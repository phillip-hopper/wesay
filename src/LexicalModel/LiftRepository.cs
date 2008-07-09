using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LiftIO;
using LiftIO.Merging;
using LiftIO.Parsing;
using LiftIO.Validation;
using Palaso.Reporting;
using WeSay.Data;
using WeSay.Foundation;

namespace WeSay.LexicalModel
{
	internal class LiftRepository : MemoryRepository<LexEntry>
	{
		private class GuidRepositoryId : RepositoryId
		{
			private readonly Guid _id;

			public GuidRepositoryId(Guid id)
			{
				_id = id;
			}

			public override int CompareTo(RepositoryId other)
			{
				return CompareTo(other as GuidRepositoryId);
			}

			public int CompareTo(GuidRepositoryId other)
			{
				if (other == null)
				{
					return 1;
				}
				return Comparer<Guid>.Default.Compare(_id, other._id);
			}

			public override bool Equals(RepositoryId other)
			{
				return Equals(other as GuidRepositoryId);
			}

			public bool Equals(GuidRepositoryId other)
			{
				if (other == null)
				{
					return false;
				}
				return Equals(_id, other._id);
			}
		}

		private readonly string _liftFilePath;
		private FileStream _liftFileStreamForLocking;
		private readonly Dictionary<GuidRepositoryId, LexEntry> _entries;

		public LiftRepository(string filePath)
		{
			_liftFilePath = filePath;
			LockLift();
			LastModified = File.GetLastWriteTimeUtc(_liftFilePath);
			_entries = new Dictionary<GuidRepositoryId, LexEntry>();
			LoadAllLexEntries();
		}

		public override LexEntry CreateItem()
		{
			LexEntry item = base.CreateItem();
			UpdateLiftFile(item);
			return item;
		}

		public override int CountAllItems()
		{
			return base.CountAllItems();
		}

		public override RepositoryId GetId(LexEntry item)
		{
			return base.GetId(item);
		}

		public override LexEntry GetItem(RepositoryId id)
		{
			return base.GetItem(id);
		}

		public override void DeleteItem(LexEntry item)
		{
			base.DeleteItem(item);
		}

		public override void DeleteItem(RepositoryId id)
		{
			base.DeleteItem(id);
		}

		public override RepositoryId[] GetAllItems()
		{
			return base.GetAllItems();
		}

		public override void SaveItem(LexEntry item)
		{
			base.SaveItem(item);
		}

		public override bool CanQuery
		{
			get { return false; }
		}

		public override bool CanPersist
		{
			get { return true; }
		}

		public override void SaveItems(IEnumerable<LexEntry> items)
		{
			base.SaveItems(items);
		}

		public override ResultSet<LexEntry> GetItemsMatching(Query query)
		{
			throw new NotSupportedException("Querying is not supported");
		}

		private void LoadAllLexEntries()
		{
			_entries.Clear();
			using (LiftMerger merger = new LiftMerger())
			{
				merger.EntryCreatedEvent += OnEntryCreated;
				LiftParser<WeSayDataObject, LexEntry, LexSense, LexExampleSentence> parser =
					new LiftParser<WeSayDataObject, LexEntry, LexSense, LexExampleSentence>(
						merger);

				parser.SetTotalNumberSteps += parser_SetTotalNumberSteps;
				parser.SetStepsCompleted += parser_SetStepsCompleted;

				parser.ParsingWarning += parser_ParsingWarning;

				try
				{
					parser.ReadLiftFile(_liftFilePath);
				}
				catch (Exception)
				{
					//our parser failed.  Hopefully, because of bad lift. Validate it now  to
					//see if that's the problem.
					Validator.CheckLiftWithPossibleThrow(_liftFilePath);

					//if it got past that, ok, send along the error the parser encountered.
					throw;
				}
			}
		}

		public override void Dispose()
		{
			UnLockLift();
			base.Dispose();
		}

		private void OnEntryCreated(object sender, LiftMerger.EntryCreatedEventArgs e)
		{
			_entries.Add(new GuidRepositoryId(e.Entry.Guid),
						 e.Entry);
		}

		private void parser_ParsingWarning(object sender,
										   LiftParser<WeSayDataObject, LexEntry, LexSense, LexExampleSentence>.ErrorArgs
											   e)
		{
		}

		private void parser_SetStepsCompleted(object sender,
											  LiftParser<WeSayDataObject, LexEntry, LexSense, LexExampleSentence>.
												  ProgressEventArgs e)
		{
		}

		private void parser_SetTotalNumberSteps(object sender,
												LiftParser<WeSayDataObject, LexEntry, LexSense, LexExampleSentence>.
													StepsArgs e)
		{
		}

		//private DateTime _timeOfLastQueryForNewRecords;

		private string LiftDirectory
		{
			get { return Path.GetDirectoryName(_liftFilePath); }
		}

		//Don't think this is needed anymore TA 7-4-2008
		///// <summary>
		///// Give a chance to do incremental update if warranted
		///// </summary>
		///// <remarks>
		///// If the caller doesn't know when actual comitts happen, that's ok.
		///// Just call at reasonable intervals.
		///// </remarks>
		///// <param name="sender"></param>
		///// <param name="e"></param>
		//public void OnDataCommitted(object sender, EventArgs e)
		//{
		//    _commitCount++;
		//    if (_commitCount < _checkFrequency)
		//    {
		//        return;
		//    }
		//    _commitCount = 0;
		//    DoLiftUpdateNow(false);
		//}

		public void OnDataDeleted(object sender, EventArgs e)
		{
			LexEntry entry = (LexEntry) sender;
			if (entry == null)
			{
				return;
			}

			LiftExporter exporter =
				new LiftExporter( /*WeSayWordsProject.Project.GetFieldToOptionListNameDictionary(), */
					MakeIncrementFileName(DateTime.UtcNow));
			exporter.AddDeletedEntry(entry);
			exporter.End();
		}

		private void UpdateLiftFile(IEnumerable<LexEntry> entriesToUpdate)
		{
			CreateIncrementFile(entriesToUpdate);
			MergeIncrementFiles();
		}

		private void UpdateLiftFile(LexEntry entryToUpdate)
		{
			CreateIncrementFile(entryToUpdate);
			MergeIncrementFiles();
		}

		private void CreateIncrementFile(LexEntry entryToUpdate)
		{
			LiftExporter exporter = new LiftExporter(MakeIncrementFileName(PreciseDateTime.UtcNow));
			//!!!exporter.Start(); //!!! Would be nice to have this CJP 2008-07-09
			exporter.Add(entryToUpdate);
			exporter.End();

			RecordUpdateTime(PreciseDateTime.UtcNow); //Why do we need to call this??? TA 7-4-2008
		}

		private void CreateIncrementFile(IEnumerable<LexEntry> entriesToUpdate)
		{
				LiftExporter exporter = null;
				foreach (LexEntry entry in entriesToUpdate)
				{
					if (exporter == null)
					{
						exporter =
							new LiftExporter(MakeIncrementFileName(PreciseDateTime.UtcNow));
						//!!!exporter.Start(); //!!! Would be nice to have this CJP 2008-07-09
					}
					exporter.Add(entry);
				}
				if (exporter != null)
				{
					exporter.End();
				}

				RecordUpdateTime(PreciseDateTime.UtcNow); //Why do we need to call this??? TA 7-4-2008
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>false if it failed (and it would have already reported the error)</returns>
		public bool MergeIncrementFiles()
		{
			//merge the increment files

			if (
				SynchronicMerger.GetPendingUpdateFiles(_liftFilePath)
					.Length > 0)
			{
				//Logger.WriteEvent("Running Synchronic Merger"); //needed??? TA 2008-07-09
				try
				{
					SynchronicMerger merger = new SynchronicMerger();
					UnLockLift();
					merger.MergeUpdatesIntoFile(_liftFilePath);
				}
				catch (BadUpdateFileException error)
				{
					string contents = File.ReadAllText(error.PathToNewFile);
					if (contents.Trim().Length == 0)
					{
						ErrorReport.ReportNonFatalMessage(
							"It looks as though WeSay recently crashed while attempting to save.  It will try again to preserve your work, but you will want to check to make sure nothing was lost.");
						File.Delete(error.PathToNewFile);
					}
					else
					{
						File.Move(error.PathToNewFile, error.PathToNewFile + ".bad");
						ErrorReport.ReportNonFatalMessage(
							"WeSay was unable to save some work you did in the previous session.  The work might be recoverable from the file {0}. The next screen will allow you to send a report of this to the developers.",
							error.PathToNewFile + ".bad");
						ErrorNotificationDialog.ReportException(error, null, false);
					}
					return false;
				}
				catch (Exception e)
				{
					throw new ApplicationException(
						"Could not finish updating LIFT dictionary file.", e);
				}
				finally
				{
					LockLift();
				}
			}
			return true;
		}

		private string MakeIncrementFileName(DateTime time)
		{
			while (true)
			{
				string timeString = time.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss'-'FFFFFFF UTC");
				string path = Path.Combine(LiftDirectory, timeString);
				path += SynchronicMerger.ExtensionOfIncrementalFiles;
				if (!File.Exists(path))
				{
					return path;
				}
				time = time.AddTicks(1);
			}
		}

		//I don't think this is needed anymore TA 7-9-2008
		///// <summary>
		///// wierd name!
		///// </summary>
		//public static void LiftIsFreshNow()
		//{
		//    RecordUpdateTime(DateTime.UtcNow);
		//}

		//What is this method for??? TA 7-4-2008
		private void RecordUpdateTime(DateTime time)
		{
			//// the resolution of the file modified time is a whole second on linux
			//// so we need to set this to the ceiling of the time in seconds and then
			//// wait until the actual time has passed this window
			//int millisecondsLostInResolution = 1000 - time.Millisecond;
			//time = time.AddMilliseconds(millisecondsLostInResolution);
			//TimeSpan timeout = time - DateTime.UtcNow;
			//if(timeout.Ticks > 0)
			//{
			//    Thread.Sleep(timeout);
			//}
			bool wasLocked = LiftIsLocked;
			if (wasLocked)
			{
				UnLockLift();
			}
			File.SetLastWriteTimeUtc(_liftFilePath, time);
			//Debug.Assert(time == GetLastUpdateTime());
			if (wasLocked)
			{
				LockLift();
			}
		}

		//I don't think this is needed anymore!!! TA 7-9-2008
		//private static DateTime GetLastUpdateTime()
		//{
		//    Debug.Assert(Directory.Exists(LiftDirectory));
		//    return File.GetLastWriteTimeUtc(_liftFilePath);
		//}


		//I don't think this is needed anymore TA 7-9-2008
		//public IList<RepositoryId> GetRecordsNeedingUpdateInLift()
		//{
		//    DateTime last = GetLastUpdateTime();
		//    _timeOfLastQueryForNewRecords = DateTime.UtcNow;
		//    return _lexEntryRepository.GetItemsModifiedSince(last);
		//}

		/// <remark>
		/// The protection provided by this simple approach is obviously limited;
		/// it will keep the lift file safe normally... but could lead to non-data-losing crashes
		/// if some automated process was sitting out there, just waiting to open as soon as we realease
		/// </summary>
		public void UnLockLift()
		{
			Debug.Assert(_liftFileStreamForLocking != null);
			_liftFileStreamForLocking.Close();
			_liftFileStreamForLocking.Dispose();
			_liftFileStreamForLocking = null;
		}

		public bool LiftIsLocked
		{
			get { return _liftFileStreamForLocking != null; }
		}

		public void LockLift()
		{
			Debug.Assert(_liftFileStreamForLocking == null);
			_liftFileStreamForLocking = File.OpenRead(_liftFilePath);
		}
	}
}