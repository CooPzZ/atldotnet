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
    public class MP4 : ILogDevice
    {
        readonly string fileToTestOn = @"M:\Temp\Audio\ATLTestOn.m4b";
        readonly string shortBook = @"M:\Temp\Audio\TestFromABC-Orig.m4b";
        //readonly string shortBook = @"M:\Temp\Audio\TestMusicFile.mp3";
        readonly string longBook = @"M:\Temp\Audio\LongBook-Orig.m4b";
        readonly string imagePath1 = @"M:\Temp\Audio\image1.jpg";
        readonly string imagePath2 = @"M:\Temp\Audio\image2.jpg";
        readonly string imagePath3 = @"M:\Temp\Audio\image3.jpg";
        readonly string imagePath4 = @"M:\Temp\Audio\image4.jpg";

        readonly Log theLog = new Log();
        readonly IList<Log.LogItem> messages = new List<Log.LogItem>();

        readonly MetaDataIOFactory.TagType removeTagsBy = MetaDataIOFactory.TagType.ANY;

        #region Init
        public MP4()
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


        #region Remove Tags
        [TestMethod]
        public void CS_RemoveTagSmallBook()
        {
            string origFile = shortBook;

            //Expected results
            TestTrackTag resultTag = new TestTrackTag(origFile);
            resultTag.ClearMeta();
            resultTag.FileSize = 9526123; //smaller than before
            resultTag.AudioDataOffset = 101922; //adjusted to removed tags
            resultTag.AudioDataSize = 9424209; //not sure why this changes? something to do with chapter info in the chapter atoms
            resultTag.Title = "ATLTestOn";

            RemoveTag(origFile, resultTag);
            //Assert.AreEqual(resultTag.FileSize, FileLength(origFile), "File should be 9623601."); //Don in the resultTag.Assertions.
        }
        [TestMethod]
        public void CS_RemoveTagLongBook()
        {
            string origFile = longBook;

            //Expected results
            TestTrackTag resultTag = new TestTrackTag(origFile);
            resultTag.ClearMeta();
            resultTag.FileSize = 579233175; // smaller than before
            resultTag.AudioDataOffset = 3107825; // adjusted to removed tags
            resultTag.AudioDataSize = 576125358; //not sure why this changes? something to do with chapter info in the chapter atoms
            resultTag.Title = "ATLTestOn";
            //resultTag.IsVBR = true; //#FAILS: Not sure why this flips -- uncomment to skip issue.

            RemoveTag(origFile, resultTag);


        }
        /// <summary>
        /// Remove Tag from specific file and test output
        /// </summary>
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
            resultTag.FileSize = 16944996; // ; //longer than before with added images was 9623601
            resultTag.AudioDataSize = 16745214; //bigger than before with added images was 9424291
            resultTag.AudioDataOffset = 199782; //adjusted with further tag details?? was 199310

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

        #region Editing Chapter Pics
        /// <summary>
        /// Add Chap Images, Save, Clear Tag - should be the same as clearing tags [CS_RemoveTagSmallBook]?
        /// Assumption: data offest has moved backwards 8 bits due to removing the whole tag?
        /// </summary>
        [TestMethod]
        public void CS_AddChapImage2_then_ClearTag_SmallBook()
        {   
            string origFile = shortBook;

            //Expected results
            TestTrackTag resultTag = new TestTrackTag(origFile);
            resultTag.ClearMeta();
            resultTag.FileSize = 9526115; //should be the same as CS_RemoveTagSmallBook-8 due to removing a buffer.
            resultTag.AudioDataOffset = 101914; //should be the same as CS_RemoveTagSmallBook - but getting 101914
            resultTag.AudioDataSize = 9424209; //should be the same as CS_RemoveTagSmallBook - almost twice the size now?? getting: 16745214.
            resultTag.Title = "ATLTestOn"; //on clear Track uses file name for Title

            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy(origFile, fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            TestTrackTag origTag = new TestTrackTag(theFile);
            OutputDebugDetails(theFile, "PRE", true);

            //Add Chap Images
            theFile.Chapters[0].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath1));
            theFile.Chapters[1].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath2));
            theFile.Save();

            //Remove Tag
            theFile = new Track(fileToTestOn);
            theFile.Remove(MetaDataIOFactory.TagType.NATIVE);

            Track afterFile = new Track(fileToTestOn);
            TestTrackTag afterTag = new TestTrackTag(afterFile);
            OutputDebugDetails(afterFile, "POST", true);
            resultTag.Assertions(new TestTrackTag(afterFile));
        }


        /// <summary>
        /// Add Chap Images, Save, Remove Chap Images manually, Save - should be the same yes?
        /// #FAILING: filesize has moved forward 1 bit??
        /// </summary>
        [TestMethod]
        public void CS_AddChapImage2_then_RemoveChapImages_SmallBook()
        {   
            string origFile = shortBook;
            TestTrackTag resultTag = new TestTrackTag(origFile);

            string fileToTestOn = "M:\\Temp\\Audio\\TestATLRun.m4b";
            System.IO.File.Delete(fileToTestOn);
            System.IO.File.Copy(origFile, fileToTestOn);
            Track theFile = new Track(fileToTestOn);
            TestTrackTag origTag = new TestTrackTag(theFile);
            OutputDebugDetails(theFile, "PRE");

            //Add Chap Images
            theFile.Chapters[0].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath1));
            theFile.Chapters[1].Picture = PictureInfo.fromBinaryData(System.IO.File.ReadAllBytes(imagePath2));
            theFile.Save();

            //Remove Tag
            theFile = new Track(fileToTestOn);
            theFile.Chapters[0].Picture = null;
            theFile.Chapters[1].Picture = null;
            theFile.Save();

            Track afterFile = new Track(fileToTestOn);
            TestTrackTag afterTag = new TestTrackTag(afterFile);
            OutputDebugDetails(afterFile, "POST");
            resultTag.Assertions(new TestTrackTag(afterFile));
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

    #region TestTrackTag
    public class TestTrackTag
    {
        public TestTrackTag() : base() { }
        public TestTrackTag(string file) : this()
        {
            Load(new Track(file));
        }
        public TestTrackTag(Track file) : this()
        {
            Load(file);
        }
        public void Load(Track file)
        {
            Title = file.Title;
            TrackNo = file.TrackNumber;
            DiscNo = file.DiscNumber;
            Artist = file.Artist;
            Comments = file.Comment;
            Description = file.Description;
            Composer = file.Composer;
            Genre = file.Genre;
            Year = file.Year;
            Album = file.Album;
            AlbumArtist = file.AlbumArtist;
            Copyright = file.Copyright;
            Publisher = file.Publisher;
            PublishDate = file.PublishingDate;
            if (file.EmbeddedPictures.Count > 0) MainPicture = file.EmbeddedPictures[0].PictureData;

            DurationMs = file.DurationMs;
            SampleRate = file.SampleRate;
            IsVBR = file.IsVBR;
            AudioFormat = file.AudioFormat.Name;
            AudioDataOffset = file.TechnicalInformation.AudioDataOffset;
            AudioDataSize = file.TechnicalInformation.AudioDataSize;
            FileSize = TrackObject.MP4.FileLength(file.Path);

            if (file.Chapters != null && file.Chapters.Count > 0)
            {
                Chapters = new List<TrackChapter>();
                foreach (ChapterInfo o in file.Chapters)
                {
                    Chapters.Add(new TrackChapter(o));
                }

            }
        }

        public void ClearMeta()
        {
            Title = "";
            TrackNo = 0;
            DiscNo = 0;
            Artist = "";
            Comments = "";
            Description = "";
            Composer = "";
            Genre = "";
            Year = 0;
            Album = "";
            AlbumArtist = "";
            Copyright = "";
            Publisher = "";
            PublishDate = new System.DateTime();
            MainPicture = null;

            //this.DurationMs = file.DurationMs;
            //this.SampleRate = file.SampleRate;
            //this.IsVBR = file.IsVBR;
            //this.AudioFormat = file.AudioFormat.Name;
            //this.AudioDataOffset = file.TechnicalInformation.AudioDataOffset;
            //this.AudioDataSize = file.TechnicalInformation.AudioDataSize;
            //this.FileSize = zzCodeSnippets.FileLength(file.Path);

            Chapters = null;
        }

        public string Title;
        public int? TrackNo;
        public int? DiscNo;
        public string Artist;
        public string Comments;
        public string Description;
        public string Composer;
        public string Genre;
        public int? Year;
        public string Album;
        public string AlbumArtist;
        public string Copyright;
        public string Publisher;
        public System.DateTime? PublishDate;
        public byte[] MainPicture;
        public IList<TrackChapter> Chapters;

        public double DurationMs;
        public double SampleRate;
        public bool IsVBR;
        public string AudioFormat;
        public string TechnicalDetails;
        public double AudioDataOffset;
        public double AudioDataSize;
        public double FileSize;

        public void Assertions(TestTrackTag Against)
        {
            Assert.AreEqual(Title, Against.Title, "Title not as expected.");
            Assert.AreEqual(TrackNo, Against.TrackNo, "TrackNo not as expected.");
            Assert.AreEqual(DiscNo, Against.DiscNo, "DiscNo not as expected.");
            Assert.AreEqual(Artist, Against.Artist, "Artist not as expected.");
            Assert.AreEqual(Comments, Against.Comments, "Comments not as expected.");
            Assert.AreEqual(Description, Against.Description, "Description not as expected.");
            Assert.AreEqual(Composer, Against.Composer, "Composer not as expected.");
            Assert.AreEqual(Genre, Against.Genre, "Genre not as expected.");
            Assert.AreEqual(Year, Against.Year, "Year not as expected.");
            Assert.AreEqual(Album, Against.Album, "Album not as expected.");
            Assert.AreEqual(AlbumArtist, Against.AlbumArtist, "AlbumArtist not as expected.");
            Assert.AreEqual(Copyright, Against.Copyright, "Copyright not as expected.");
            Assert.AreEqual(Publisher, Against.Publisher, "Publisher not as expected.");
            Assert.AreEqual(PublishDate, Against.PublishDate, "PublishDate not as expected.");
            Assert.IsTrue(CheckBytesAreSame(MainPicture, Against.MainPicture), "MainPicture not as expected.");

            Assert.AreEqual(DurationMs, Against.DurationMs, "DurationMs not as expected.");
            Assert.AreEqual(SampleRate, Against.SampleRate, "SampleRate not as expected.");
            Assert.AreEqual(IsVBR, Against.IsVBR, "IsVBR not as expected.");
            Assert.AreEqual(AudioFormat, Against.AudioFormat, "AudioFormat not as expected.");
            Assert.AreEqual(TechnicalDetails, Against.TechnicalDetails, "TechnicalDetails not as expected.");
            Assert.AreEqual(FileSize, Against.FileSize, "FileSize not as expected.");
            Assert.AreEqual(AudioDataOffset, Against.AudioDataOffset, "AudioDataOffset not as expected.");
            Assert.AreEqual(AudioDataSize, Against.AudioDataSize, "AudioDataSize not as expected.");

            //Assert.IsFalse(this.Chapters != null && Against.Chapters != null, "Chapters not as expected.");
            if (Chapters != null && Chapters.Count > 0)
            {
                Assert.AreEqual(Chapters.Count, Against.Chapters.Count, "Chapter Count not as expected.");

                for (int n = 0; n <= Chapters.Count - 1; n++)
                {
                    Assert.AreEqual(Chapters[n].StartTime, Against.Chapters[n].StartTime, $"Chapter {n} StartTime not as expected.");
                    Assert.AreEqual(Chapters[n].EndTime, Against.Chapters[n].EndTime, $"Chapter {n} EndTime not as expected.");
                    Assert.AreEqual(Chapters[n].StartOffset, Against.Chapters[n].StartOffset, $"Chapter {n} StartOffset not as expected.");
                    Assert.AreEqual(Chapters[n].EndOffset, Against.Chapters[n].EndOffset, $"Chapter {n} EndOffset not as expected.");
                    Assert.AreEqual(Chapters[n].UseOffset, Against.Chapters[n].UseOffset, $"Chapter {n} UseOffset not as expected.");
                    Assert.AreEqual(Chapters[n].UniqueID, Against.Chapters[n].UniqueID, $"Chapter {n} UniqueID not as expected.");
                    Assert.AreEqual(Chapters[n].Subtitle, Against.Chapters[n].Subtitle, $"Chapter {n} Subtitle not as expected.");
                    Assert.IsTrue(CheckBytesAreSame(Chapters[n].PictureData, Against.Chapters[n].PictureData), $"Chapter {n} Picture not as expected.");
                }

            }
            else if (Against.Chapters != null && Against.Chapters.Count > 0)
            {
                Assert.Fail("Chapters not as expected.");
            }

        }
        private bool CheckBytesAreSame(byte[] a, byte[] b)
        {
            if (a == null && b != null) return false;
            if (b == null && a != null) return false;
            if (a == null && b == null) return true;
            if (a.Length != b.Length) return false;

            for (long n = 0; n < a.Length; n++)
                if (a[n] != b[n]) return false;

            return true;
        }


        public string ToDebugString()
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();

            s.AppendLine($"DurationMs: {DurationMs}");
            s.AppendLine($"SampleRate: {SampleRate}");
            s.AppendLine($"IsVBR: {IsVBR}");
            s.AppendLine($"AudioFormat: {AudioFormat}");
            s.AppendLine($"TechnicalDetails: {TechnicalDetails}");
            s.AppendLine($"AudioDataOffset: {AudioDataOffset}");
            s.AppendLine($"AudioDataSize: {AudioDataSize}");
            s.AppendLine($"FileSize: {FileSize}");

            s.AppendLine($"Title: {Title}");
            s.AppendLine($"TrackNo: {TrackNo}");
            s.AppendLine($"DiscNo: {DiscNo}");
            s.AppendLine($"Artist: {Artist}");
            s.AppendLine($"Comments: {Comments}");
            s.AppendLine($"Description: {Description}");
            s.AppendLine($"Composer: {Composer}");
            s.AppendLine($"Genre: {Genre}");
            s.AppendLine($"Year: {Year}");
            s.AppendLine($"Album: {Album}");
            s.AppendLine($"AlbumArtist: {AlbumArtist}");
            s.AppendLine($"Copyright: {Copyright}");
            s.AppendLine($"Publisher: {Publisher}");
            s.AppendLine($"PublishDate: {PublishDate}");
            s.AppendLine($"MainPicture: {MainPicture?.Length}");

            if (Chapters == null)
                s.AppendLine($"Chapters: null");
            else
            {
                s.AppendLine($"Chapters: {Chapters.Count}");
                if (Chapters.Count > 0)
                {
                    for (int n = 0; n <= Chapters.Count - 1; n++)
                    {
                        s.AppendLine($"> 1:");
                        s.AppendLine($"StartTime: {Chapters[n].StartTime}");
                        s.AppendLine($"EndTime: {Chapters[n].EndTime}");
                        s.AppendLine($"StartOffset: {Chapters[n].StartOffset}");
                        s.AppendLine($"EndOffset: {Chapters[n].EndOffset}");
                        s.AppendLine($"UseOffset: {Chapters[n].UseOffset}");
                        s.AppendLine($"UniqueID: {Chapters[n].UniqueID}");
                        s.AppendLine($"Subtitle: {Chapters[n].Subtitle}");
                        if (Chapters[n].PictureData == null)
                            s.AppendLine($"PictureData: null");
                        else
                        {
                            s.AppendLine($"PictureData: {Chapters[n].PictureData.Length}");
                            s.AppendLine($"PictureHash: {Chapters[n].PictureHash}");
                        }

                    }
                }
            }

            return s.ToString();
        }
    }
    public class TrackChapter
    {
        public TrackChapter() : base()
        { }

        public TrackChapter(ChapterInfo c)
        {
            StartTime = c.StartTime;
            EndTime = c.EndTime;
            StartOffset = c.StartOffset;
            EndOffset = c.EndOffset;
            UseOffset = c.UseOffset;
            UniqueID = c.UniqueID;
            Subtitle = c.Subtitle;
            PictureData = c.Picture?.PictureData;
            PictureHash = c.Picture?.ComputePicHash();
        }

        public uint StartTime;
        public uint EndTime;
        public uint StartOffset;
        public uint EndOffset;
        public bool UseOffset;
        public string UniqueID;
        public string Subtitle;
        //public UrlInfo Url;
        public byte[] PictureData;
        public uint? PictureHash;

    }
    #endregion

}
