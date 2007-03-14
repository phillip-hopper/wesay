using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Exortech.NetReflector;
using NUnit.Framework;
using System.ComponentModel;
using WeSay.Language;
using System.Collections;

namespace WeSay.Foundation.Tests
{
    [TestFixture]
    public class MultiTextTests
    {
        private bool _gotHandlerNotice;

       [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Notification()
        {
            _gotHandlerNotice = false;
            MultiText text = new MultiText();
            text.PropertyChanged += new PropertyChangedEventHandler(propertyChangedHandler);
            text.SetAlternative("zox", "");
            Assert.IsTrue(_gotHandlerNotice);
        }

        void propertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            _gotHandlerNotice = true;
        }

        [Test]
        public void NullConditions()
        {
            MultiText text = new MultiText();
            Assert.AreSame(string.Empty, text["foo"], "never before heard of alternative should give back an empty string");
            Assert.AreSame(string.Empty, text["foo"], "second time");
            Assert.AreSame(string.Empty, text.GetBestAlternative("fox"));
            text.SetAlternative("zox", "");
            Assert.AreSame(string.Empty, text["zox"]);
            text.SetAlternative("zox", null);
            Assert.AreSame(string.Empty, text["zox"], "should still be empty string after setting to null");
            text.SetAlternative("zox", "something");
            text.SetAlternative("zox", null);
            Assert.AreSame(string.Empty, text["zox"], "should still be empty string after setting something and then back to null");
        }
        [Test]
        public void BasicStuff()
        {
            MultiText text = new MultiText();
            text["foo"] = "alpha";
            Assert.AreSame("alpha", text["foo"]);
            text["foo"] = "beta";
            Assert.AreSame("beta", text["foo"]);
            text["foo"] = "gamma";
            Assert.AreSame("gamma", text["foo"]);
            text["bee"] = "beeeee";
            Assert.AreSame("gamma", text["foo"], "setting a different alternative should not affect this one");
            text["foo"] = null;
            Assert.AreSame(string.Empty, text["foo"]);
        }

//        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
//        public void GetIndexerThrowsWhenAltIsMissing()
//        {
//            MultiText text = new MultiText();
//            text["foo"] = "alpha";
//            string s = text["gee"];
//        }
//
//        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
//        public void GetExactThrowsWhenAltIsMissing()
//        {
//            MultiText text = new MultiText();
//            text["foo"] = "alpha";
//            string s = text.GetExactAlternative("gee");
//        }

//        [Test]
//        public void ImplementsIEnumerable()
//        {
//            MultiText text = new MultiText();
//            IEnumerable ienumerable = text;
//            Assert.IsNotNull(ienumerable);
//        }

        [Test]
        public void Count()
        {
            MultiText text = new MultiText();
            Assert.AreEqual(0, text.Count);
            text["a"] = "alpha";
            text["b"] = "beta";
            text["g"] = "gamma";
            Assert.AreEqual(3, text.Count);
        }

        [Test]
        public void IterateWithForEach()
        {
            MultiText text = new MultiText();
            text["a"] = "alpha";
            text["b"] = "beta";
            text["g"] = "gamma";
            int i = 0;
            foreach (LanguageForm l in text)
            {
                switch(i){
                    case 0:
                        Assert.AreEqual("a", l.WritingSystemId);
                        Assert.AreEqual("alpha", l.Form);
                        break;
                    case 1:
                        Assert.AreEqual("b", l.WritingSystemId);
                        Assert.AreEqual("beta", l.Form);
                        break;
                    case 2:
                        Assert.AreEqual("g", l.WritingSystemId);
                        Assert.AreEqual("gamma", l.Form);
                        break;
                }
                i++;
            }
        }
        [Test]
        public void GetEnumerator()
        {
            MultiText text = new MultiText();
            IEnumerator ienumerator = text.GetEnumerator();
            Assert.IsNotNull(ienumerator);
        }


        [Test]
        public void MergedGuyHasCorrectParentsOnForms()
        {
            MultiText x = new MultiText();
            x["a"] = "alpha";
            MultiText y = new MultiText();
            y["b"] = "beta";
            x.MergeIn(y);
            Assert.AreSame(y, y.Find("b").Parent);
            Assert.AreSame(x, x.Find("b").Parent);
        }
        

        [Test]
        public void MergeWithEmpty()
        {
            MultiText old = new MultiText();
            MultiText newGuy = new MultiText();
            old.MergeIn(newGuy);
            Assert.AreEqual(0, old.Count);

            old = new MultiText();
            old["a"] = "alpha";
            old.MergeIn(newGuy);
            Assert.AreEqual(1, old.Count);
        }

        [Test]
        public void MergeWithOverlap()
        {
            MultiText old = new MultiText();
            old["a"] = "alpha";
            old["b"] = "beta";
            MultiText newGuy = new MultiText();
            newGuy["b"] = "newbeta";
            newGuy["c"] = "charlie";
            old.MergeIn(newGuy);
            Assert.AreEqual(3, old.Count);
            Assert.AreEqual("newbeta", old["b"]);
        }

        [Test]
        public void UsesNextAlternativeWhenMissing()
        {
            MultiText multiText = new MultiText();
            multiText["wsWithNullElement"] = null;
             multiText["wsWithEmptyElement"] = "";
           multiText["wsWithContent"] = "hello";
           Assert.AreEqual(String.Empty, multiText.GetExactAlternative("missingWs"));
           Assert.AreEqual(String.Empty, multiText.GetExactAlternative("wsWithEmptyElement"));
            Assert.AreEqual("hello", multiText.GetBestAlternative("missingWs"));
            Assert.AreEqual("hello", multiText.GetBestAlternative("wsWithEmptyElement"));
            Assert.AreEqual("hello*", multiText.GetBestAlternative("wsWithEmptyElement", "*"));
            Assert.AreEqual("hello", multiText.GetBestAlternative("wsWithNullElement"));
            Assert.AreEqual("hello*", multiText.GetBestAlternative("wsWithNullElement", "*"));
            Assert.AreEqual("hello", multiText.GetExactAlternative("wsWithContent"));
            Assert.AreEqual("hello", multiText.GetBestAlternative("wsWithContent"));
            Assert.AreEqual("hello", multiText.GetBestAlternative("wsWithContent", "*"));
      }

        [Test]
        public void SetAnnotation()
        {
           MultiText multiText = new MultiText();
            multiText.SetAnnotationOfAlternativeIsStarred("zz", true);
           Assert.AreEqual(String.Empty, multiText.GetExactAlternative("zz"));
           Assert.IsTrue(multiText.GetAnnotationOfAlternativeIsStarred("zz"));
           multiText.SetAnnotationOfAlternativeIsStarred("zz", false);
           Assert.IsFalse(multiText.GetAnnotationOfAlternativeIsStarred("zz"));
        }

        [Test]
        public void ClearingAnnotationOfEmptyAlternativeRemovesTheAlternative()
        {
            MultiText multiText = new MultiText();
            multiText.SetAnnotationOfAlternativeIsStarred("zz", true);
            multiText.SetAnnotationOfAlternativeIsStarred("zz", false);
            Assert.IsFalse(multiText.ContainsAlternative("zz"));
        }

        [Test]
        public void ClearingAnnotationOfNonEmptyAlternative()
        {
            MultiText multiText = new MultiText();
            multiText.SetAnnotationOfAlternativeIsStarred("zz", true);
            multiText["zz"] = "hello";
            multiText.SetAnnotationOfAlternativeIsStarred("zz", false);
            Assert.IsTrue(multiText.ContainsAlternative("zz"));
        }

        [Test]
        public void EmptyingTextOfFlaggedAlternativeDeletesEvenWithFlag()
        {
             // REVIEW: not clear really what behavior we want here, since user deletes via clearing text
            MultiText multiText = new MultiText();
            multiText["zz"] = "hello";
            multiText.SetAnnotationOfAlternativeIsStarred("zz", true);
            multiText["zz"] = "";
            Assert.IsFalse(multiText.ContainsAlternative("zz"));
        }

        [Test]
        public void AnnotationOfMisssingAlternative()
        {
            MultiText multiText = new MultiText();
            Assert.IsFalse(multiText.GetAnnotationOfAlternativeIsStarred("zz"));
            Assert.IsFalse(multiText.ContainsAlternative("zz"),"should not cause the creation of the alt");
        }

        [Test]
        public void SerializeWithXmlSerializer()
        {
            MultiText text = new MultiText();
            text["foo"] = "alpha";
            text["boo"] = "beta";

            XmlSerializer ser = new XmlSerializer(typeof(TestMultiTextHolder));
            
            StringWriter writer = new System.IO.StringWriter();
            TestMultiTextHolder holder = new TestMultiTextHolder();
            holder.Name = text;
           ser.Serialize(writer, holder);

            string mtxml = writer.GetStringBuilder().ToString();
            mtxml = mtxml.Replace('"', '\'');
           Debug.WriteLine(mtxml);
          string answer =
                @"<?xml version='1.0' encoding='utf-16'?>
<TestMultiTextHolder xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
  <name>
    <form starred='false' ws='foo'>alpha</form>
    <form starred='false' ws='boo'>beta</form>
  </name>
</TestMultiTextHolder>";
            Assert.AreEqual(answer, mtxml);
        }

        [Test]
        public void DeSerialize()
        {
            MultiText text = new MultiText();
            text["foo"] = "alpha";

            NetReflectorTypeTable t = new NetReflectorTypeTable();
            t.Add(typeof(MultiText));
            t.Add(typeof(TestMultiTextHolder));


            string answer =
                @"<testMultiTextHolder>
                    <name>
				        <form starred='false' ws='en'>verb</form>
				        <form starred='false' ws='fr'>verbe</form>
				        <form starred='false' ws='es'>verbo</form>
			        </name>
                </testMultiTextHolder>";
            NetReflectorReader r = new NetReflectorReader(t);
            TestMultiTextHolder h = (TestMultiTextHolder)r.Read(answer);
            Assert.AreEqual(3, h._name.Count);
            Assert.AreEqual("verbo",h._name["es"]);
        }

        [ReflectorType("testMultiTextHolder")]
        public class TestMultiTextHolder
        {
            [XmlIgnore]
            public MultiText _name;

           [XmlElement("name")]
            [ReflectorProperty("name", typeof(MultiTextSerializorFactory), Required = true)]
            public MultiText Name
            {
                get { return _name; }
                set { _name = value; }
            }
        }
    }
}

