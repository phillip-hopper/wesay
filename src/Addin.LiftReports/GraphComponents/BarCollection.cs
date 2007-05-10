#region copyright info
//
// Written by Anup. V (anupshubha@yahoo.com)
// Copyright (c) 2006.
//
// This code may be used in compiled form in any way you desire. This
// file may be redistributed by any means PROVIDING it is not sold for
// for profit without the authors written consent, and providing that
// this notice and the authors name is included. If the source code in
// this file is used in any commercial application then acknowledgement
// must be made to the author of this file (in whatever form you wish).
//
// This file is provided "as is" with no expressed or implied warranty.
//
// Please use and enjoy. Please let me know of any bugs/mods/improvements
// that you have found/implemented and I will fix/incorporate them into
// this file.
#endregion copyright info

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace GraphComponents
{
	/// <summary>
	/// This class provides a mechanism for the user to treat the bars of
	/// the stacked bar graphs as a single collection.
	/// </summary>
	public class BarCollection : CollectionBase
	{
		public BarCollection ()
		{
		}

		/// <summary>
		/// Gets or sets the Bar object at the particular index
		/// </summary>
		public Bar this [int index]
		{
			get
			{
				return (Bar)List[index];
			}
			set
			{
				List[index] = value;
			}
		}

		/// <summary>
		/// Adds a new bar into the stacked bar graph
		/// </summary>
		public int Add( Bar value )
		{
			if (value == null)
				throw new ArgumentNullException ();

			return( List.Add( value ) );
		}

		/// <summary>
		/// Returns the index of a specified bar in the stacked bar graph
		/// </summary>
		public int IndexOf( Bar value )
		{
			return( List.IndexOf( value ) );
		}

		/// <summary>
		/// Inserts a bar in the index specified in the stacked bar graph
		/// </summary>
		public void Insert( int index, Bar value )
		{
			if (value == null || index >= List.Count)
				return;

			List.Insert( index, value );
		}

		/// <summary>
		/// Removes the specified bar from the stacked bar graph
		/// </summary>
		public void Remove ( Bar value )
		{
			if (value == null)
				return;

			if (! List.Contains (value))
				throw new ArgumentException ();

			for (int i = 0; i < List.Count; i ++)
			{
				if ( ((Bar) List[i]).Name == value.Name)
				{
					List.RemoveAt (i);
					break;
				}
			}
		}

		/// <summary>
		/// Checks whether a bar is contained in the stacked bar graph
		/// </summary>
		/// <param name="value">Bar to check for</param>
		/// <returns>True if it is in the stacked bar graph. False otherwise</returns>
		public bool Contains( Bar value )
		{
			// If value is not of type Bar, this will return false.
			return( List.Contains( value ) );
		}

		protected override void OnInsert( int index, Object value )
		{
			// Insert additional code to be run only when inserting values.
		}

		protected override void OnRemove( int index, Object value )
		{
			// Insert additional code to be run only when removing values.
		}

		protected override void OnSet( int index, Object oldValue, Object newValue )
		{
			// Insert additional code to be run only when setting values.
		}

		protected override void OnValidate( Object value )
		{
			if ( value.GetType() != Type.GetType("GraphComponents.Bar") )
				throw new ArgumentException ();
		}

		public static void CopyTo (BarCollection collection, System.Int32 index)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");

			if (index < 0 || index >= collection.Count)
				throw new ArgumentOutOfRangeException ("index");

			// TODO: Implement this later
			throw new InvalidOperationException ();
		}

	}
}
