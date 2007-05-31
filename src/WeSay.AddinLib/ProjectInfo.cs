namespace WeSay.AddinLib
{
	public class ProjectInfo
	{

		private string _pathToTopLevelDirectory;
		private string _pathToLIFT;
		private string _name;
		private string[] _filesBelongingToProject;


		private FileLocater _locateFile;

		public ProjectInfo(string name, string pathToTopLevelDirectory, string pathToLIFT, string[] filesBelongingToProject, FileLocater locateFile)
		{
			_pathToTopLevelDirectory = pathToTopLevelDirectory;
			_locateFile = locateFile;
			_filesBelongingToProject = filesBelongingToProject;
			_name = name;
			_pathToLIFT = pathToLIFT;
		}

		/// <summary>
		/// Find the file, starting with the project dirs and moving to the app dirs.
		/// This allows a user to override an installed file by making thier own.
		/// </summary>
		public string LocateFile(string fileName)
		{
			return _locateFile(fileName);
		}

		public string PathToLIFT
		{
			get
			{
				return _pathToLIFT;
			}
		}

		public string PathToTopLevelDirectory
		{
			get
			{
				return _pathToTopLevelDirectory;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}


		public string[] FilesBelongingToProject
		{
			get
			{
				return _filesBelongingToProject;
			}
		}
	}
}