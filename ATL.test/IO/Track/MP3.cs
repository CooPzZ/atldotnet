using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATL.AudioData;
using ATL.CatalogDataReaders;
using ATL.Logging;
using System.Collections.Generic;
using ATL.Playlist;
using Commons;

namespace ATL.test.IO.TrackObject

{
#pragma warning disable S2699 // Tests should include assertions

    [TestClass]
    public class MP3 : ILogDevice
    {
        readonly string fileToTestOn = @"M:\Temp\Audio\ATLTestOn.mp3";
        readonly string shortBook = @"M:\Temp\Audio\TestBook-Orig.mp3";
        //readonly string shortBook = @"M:\Temp\Audio\TestMusicFile - Orig.mp3";
        readonly string imagePath1 = @"M:\Temp\Audio\image1.jpg";
        readonly string imagePath2 = @"M:\Temp\Audio\image2.jpg";
        readonly string imagePath3 = @"M:\Temp\Audio\image3.jpg";
        readonly string imagePath4 = @"M:\Temp\Audio\image4.jpg";

        readonly Log theLog = new Log();
        readonly IList<Log.LogItem> messages = new List<Log.LogItem>();

        readonly MetaDataIOFactory.TagType removeTagsBy = MetaDataIOFactory.TagType.ANY;

        #region Init
        public MP3()
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


        #region Test Tag editing
        [TestMethod]
        public void CS_UpdateAllFields()
        {
            string origFile = shortBook;

            //Expected results
            TestTrackTag resultTag = new TestTrackTag(origFile);
            resultTag.Album = "New Album";
            resultTag.Artist = "New Artist";
            resultTag.AlbumArtist = "New AlbumArtist";
            resultTag.Title = "New Title";
            resultTag.Description = ""; //No longer works on MP3 files - ignore
            resultTag.LongDescription = "New Long Description";
            resultTag.GroupDescription = "New Group Description";
            resultTag.Comments = "New Comment";
            resultTag.TrackNo = 43;
            resultTag.DiscNo = 22;
            resultTag.Composer = "New Composer";
            resultTag.Copyright = "New Copyright";
            resultTag.Genre = "New Genre";
            resultTag.Publisher = "New Publisher";
            resultTag.Year = 3002;
            resultTag.PublishDate = new System.DateTime();
            resultTag.MainPicture = System.IO.File.ReadAllBytes(imagePath1);

            resultTag.FileSize = 317243512; //smaller than before
            resultTag.AudioDataOffset = 3567867; //adjusted to removed tags
            //resultTag.AudioDataSize = ; //should be the same in mp3 files

            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy(origFile, fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            TestTrackTag origTag = new TestTrackTag(theFile);

            OutputDebugDetails(theFile, "PRE", true);
            resultTag.CopyTo(theFile);
            theFile.Save(); //Dont need.
            Track afterFile = new Track(fileToTestOn);
            TestTrackTag afterTag = new TestTrackTag(afterFile);
            OutputDebugDetails(afterFile, "POST", true);

            resultTag.Assertions(afterTag);

        }
        #endregion

        #region Remove Tags
        public void RemoveTag(string file, TestTrackTag resultTestTag)
        {
            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy(file, fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            TestTrackTag origTag = new TestTrackTag(theFile);

            OutputDebugDetails(theFile, "PRE", true);
            theFile.Remove(removeTagsBy);
            //theFile.Save(); //Dont need.
            Track afterFile = new Track(fileToTestOn);
            TestTrackTag afterTag = new TestTrackTag(afterFile);
            OutputDebugDetails(afterFile, "POST", true);

            Assert.AreEqual(origTag.DurationMs, afterFile.DurationMs, "Duration should be the same.");
            Assert.IsTrue(origTag.FileSize > afterTag.FileSize, "File should be smaller.");

            resultTestTag.Assertions(new TestTrackTag(afterFile));
        }
        #endregion

        #region Add Chapter Pics
        [TestMethod, TestCategory("snippets")]
        public void CS_AddChapImagesShortBook()
        {
            string origFile = shortBook;

            //Expected results
            TestTrackTag resultTag = new TestTrackTag(origFile);
            resultTag.Chapters[0].PictureData = System.IO.File.ReadAllBytes(imagePath1);
            resultTag.Chapters[1].PictureData = System.IO.File.ReadAllBytes(imagePath2);
            resultTag.FileSize = 321048146; // ; //longer than before with added images was 9623601
            //resultTag.AudioDataSize = 16745214; //should be the same in MP3 files
            resultTag.AudioDataOffset = 7372501; //adjusted with further tag details?? was 199310

            AddChapImagesFirst2(origFile, resultTag);
        }
        /// <summary>
        /// Add Images to all chapters (2) and check output
        /// </summary>
        public void AddChapImagesFirst2(string file, TestTrackTag resultTestTag)
        {
            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy(file, fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            TestTrackTag origTag = new TestTrackTag(theFile);

            OutputDebugDetails(theFile, "PRE", true);
            theFile.Chapters[0].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath1));
            theFile.Chapters[1].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath2));
            theFile.Save();
            Track afterFile = new Track(fileToTestOn);
            TestTrackTag afterTag = new TestTrackTag(afterFile);
            OutputDebugDetails(afterFile, "POST", true);

            Assert.AreEqual(origTag.DurationMs, afterFile.DurationMs, "Duration should be the same.");
            Assert.IsTrue(origTag.FileSize < afterTag.FileSize, "File should be bigger.");

            resultTestTag.Assertions(new TestTrackTag(afterFile));
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
#pragma warning restore S2699 // Tests should include assertions

}
