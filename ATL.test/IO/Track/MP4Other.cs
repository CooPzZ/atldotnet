using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATL.AudioData;
using ATL.CatalogDataReaders;
using ATL.Logging;
using System.Collections.Generic;
using ATL.Playlist;
using Commons;

namespace ATL.test.IO.TrackObject

{

    [TestClass]
    public class MP4Other : ILogDevice
    {
        readonly string fileToTestOn = @"M:\Temp\Audio\ATLTestOn.m4b";
        readonly string imagePath1 = @"M:\Temp\Audio\image1.jpg";
        readonly string imagePath2 = @"M:\Temp\Audio\image2.jpg";
        readonly string imagePath3 = @"M:\Temp\Audio\image3.jpg";
        readonly string imagePath4 = @"M:\Temp\Audio\image4.jpg";

        readonly Log theLog = new Log();
        readonly IList<Log.LogItem> messages = new List<Log.LogItem>();


        #region Init
        public MP4Other()
        {
            LogDelegator.SetLog(ref theLog);
            theLog.Register(this);
        }

        public void DoLog(Log.LogItem anItem)
        {
            messages.Add(anItem);
        }

        [TestInitialize]
        public void Init()
        {
            ATL.Settings.FileBufferSize = 2000000;
        }

        [TestCleanup]
        public void End()
        {
        }
        #endregion

        #region Other Specific Tests

        double removeTag_expectedPostLenght = 9526115;
        /// <summary>
        /// Test Track.Remove() that it removes the tag based on File details.
        /// </summary>
        [TestMethod]
        public void CS_RemoveTag()
        {
            //string fileToTestOn = audioFilePath;
            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy("M:\\Temp\\Audio\\TestFromABC-Orig.m4b", fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            double tDuration = theFile.DurationMs; System.Console.WriteLine("Pre Duration: " + tDuration);
            double dLenght = FileLength(fileToTestOn); System.Console.WriteLine("Pre File Length: " + dLenght);
            theFile.Remove(MetaDataIOFactory.TagType.NATIVE);
            //theFile.Save(); //Dont need.
            theFile = new Track(fileToTestOn);
            double dPostLenght = FileLength(fileToTestOn);
            System.Console.WriteLine("POST Duration: " + theFile.DurationMs.ToString());
            System.Console.WriteLine("POST File Length: " + dPostLenght);

            Assert.AreEqual(tDuration, theFile.DurationMs, "Duration should be the same.");
            Assert.IsTrue(dLenght > dPostLenght, "File should be smaller.");
            // 8 extra bytes because the empty padding atom (`free` atom) isn't removed by design when using Track.Remove
            // as padding areas aren't considered as metadata per se, and are kept to facilitate file expansion
            Assert.AreEqual(removeTag_expectedPostLenght+8, dPostLenght, $"File should be {removeTag_expectedPostLenght+8} once tags are removed.");
        }


        /// <summary>
        /// Test Track.Remove() after editing chapter images when chapter 1 doesnt have an image set.
        /// #FAILING: Remove does not appear to be removing the images.
        /// </summary>
        [TestMethod]
        public void CS_AddChap2Image_then_RemoveTag()
        {
            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy("M:\\Temp\\Audio\\TestFromABC-Orig.m4b", fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            System.Console.WriteLine("# Initial Details #");
            double tDuration = theFile.DurationMs; System.Console.WriteLine("Duration: " + tDuration);
            double dLenght = FileLength(fileToTestOn); System.Console.WriteLine("File Length: " + dLenght);
            System.Console.WriteLine("Chapters: " + theFile.Chapters.Count.ToString());
            System.Console.WriteLine("Chapters(1) Image: " + (theFile.Chapters[0].Picture != null));
            System.Console.WriteLine("Chapters(2) Image: " + (theFile.Chapters[1].Picture != null));

            System.Console.WriteLine("# Chap 2 Image added #");
            theFile.Chapters[1].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath2));
            theFile.Save();
            theFile = new Track(fileToTestOn);
            System.Console.WriteLine("Duration: " + theFile.DurationMs);
            System.Console.WriteLine("File Length: " + FileLength(fileToTestOn));
            System.Console.WriteLine("Chapters: " + theFile.Chapters.Count.ToString());
            System.Console.WriteLine("Chapters(1) Image: " + (theFile.Chapters[0].Picture != null));
            System.Console.WriteLine("Chapters(2) Image: " + (theFile.Chapters[1].Picture != null));

            //Switch these Assertions for expected editing.
            //Assert.IsTrue(theFile.Chapters[0].Picture == null, "Picture should not exist in Chap 1."); 
            //Assert.IsTrue(theFile.Chapters[1].Picture != null, "Picture should exist in Chap 2.");
            Assert.IsTrue(theFile.Chapters[0].Picture != null, "Picture should exist in Chap 1 due to MP4 format limitation.");
            Assert.IsTrue(theFile.Chapters[1].Picture == null, "Picture is no longer in Chap 2 due to MP4 format limitation.");

            System.Console.WriteLine("# Remove Tags #");
            theFile.Remove(MetaDataIOFactory.TagType.NATIVE);
            theFile = new Track(fileToTestOn);
            double dPostLenght = FileLength(fileToTestOn);
            System.Console.WriteLine("Duration: " + theFile.DurationMs.ToString());
            System.Console.WriteLine("File Length: " + dPostLenght);
            System.Console.WriteLine("Chapters: " + theFile.Chapters.Count.ToString());
            if (theFile.Chapters.Count > 0)
            {
                System.Console.WriteLine("Chapters(1) Image: " + (theFile.Chapters[0].Picture != null));
                System.Console.WriteLine("Chapters(2) Image: " + (theFile.Chapters[1].Picture != null));
            }

            Assert.AreEqual(tDuration, theFile.DurationMs, "Duration should be the same.");
            Assert.IsTrue(dLenght > dPostLenght, "File should be smaller.");
            Assert.AreEqual(removeTag_expectedPostLenght, dPostLenght, $"File should be {removeTag_expectedPostLenght} once tags are removed - As per test CS_RemoveTag.");
        }

        #endregion

        #region Helpers
        private void OutputDebugDetails(Track file, string tag, bool FullMeta = false)
        {
            if (FullMeta)
            {
                System.Console.WriteLine($"{tag} Meta Details >> ");
                TestTrackTag testTrack = new TestTrackTag(file);
                System.Console.WriteLine(testTrack.ToDebugString());
            }
            else
            {
                System.Console.WriteLine($"{tag} Duration > " + file.DurationMs.ToString());
                System.Console.WriteLine($"{tag} File Length > " + FileLength(file.Path));
            }

        }
        public static double FileLength(string file)
        {
            return new System.IO.FileInfo(file).Length;
        }
        #endregion
    }

}
