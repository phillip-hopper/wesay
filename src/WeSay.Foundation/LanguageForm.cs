using System;
using System.Xml.Serialization;
using Exortech.NetReflector;

namespace WeSay.Language
{
	/// <summary>
	/// A LanguageForm is a unicode string plus the id of its writing system
	/// </summary>
	[ReflectorType("alt")]
	public class LanguageForm
	{
		private string _writingSystemId;
		private string _form;
		private Annotation _annotation;

		/// <summary>
		/// See the comment on MultiText._parent for information on
		/// this field.
		/// </summary>
		private MultiText _parent;

		/// <summary>
		/// for netreflector
		/// </summary>
		public LanguageForm()
		{
		}

		public LanguageForm(string writingSystemId, string form, MultiText parent)
		{
			if (parent == null)
			{
				throw new ArgumentException("Parent cannot be null unless using for non-db4o purposes (e.g. netreflector an options)", "parent");
			}
			_parent = parent;
			_writingSystemId = writingSystemId;
			_form =  form;
		}

		[XmlAttribute("starred")]
		public bool IsStarred
		{
			get
			{
				if (_annotation == null)
				{
					return false; // don't bother making one yet
				}
				return _annotation.IsOn;
			}
			set
			{
				if (!value)
				{
					_annotation = null; //free it up
				}
				else if (_annotation == null)
				{
					_annotation = new Annotation();
					_annotation.IsOn = true;
				}
			}
		}

		[ReflectorProperty("ws", Required = true)]
		[XmlAttribute("ws")]
		public string WritingSystemId
		{
			get { return _writingSystemId; }

			///needed for depersisting with netreflector
			set
			{
				_writingSystemId = value;
			}
		}

		[ReflectorProperty("form", Required = true)]
		[XmlText]
		public string Form
		{
			get { return _form; }
			set { _form = value; }
		}

		/// <summary>
		/// See the comment on MultiText._parent for information on
		/// this field.
		/// </summary>
		[XmlIgnore]
		public MultiText Parent
		{
			get { return _parent; }
		}
	}
}