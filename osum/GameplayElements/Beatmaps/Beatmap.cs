//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using System.IO;
using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;
using osu_common.Libraries.Osz2;
namespace osum.GameplayElements.Beatmaps
{
    public partial class Beatmap : IDisposable
    {
        public string ContainerFilename;

        public byte DifficultyOverall;
        public byte DifficultyCircleSize;
        public byte DifficultyHpDrainRate;
        public int StackLeniency = 1;

        public string BeatmapFilename { get { return Package.MapFiles[0]; } }
        public string StoryboardFilename { get { return ""; } }

        public Dictionary<Difficulty, BeatmapDifficultyInfo> DifficultyInfo = new Dictionary<Difficulty, BeatmapDifficultyInfo>();

        private MapPackage package;
        public MapPackage Package
        {
            get
            {
                if (ContainerFilename == null) return null;

                try
                {
                    if (package == null && ContainerFilename.EndsWith("osz2"))
                        package = new MapPackage(ContainerFilename);
                }
                catch
                {
                    return null;
                }

                return package;
            }

        }
        public string AudioFilename = "audio.mp3";
        public List<int> StreamSwitchPoints;

        public Beatmap()
        {
        }

        public Beatmap(string containerFilename)
        {
            ContainerFilename = containerFilename;
        }

        public Stream GetFileStream(string filename)
        {
            if (ContainerFilename == null)
                return new FileStream(
                    (ContainerFilename.EndsWith(".osc") ? Environment.GetFolderPath(Environment.SpecialFolder.Personal) : ContainerFilename) + "/" + filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            MapPackage p = Package;
            if (p == null) return null;
            return p.GetFile(filename);
        }

        internal byte[] GetFileBytes(string filename)
        {
            byte[] data = null;

            using (Stream stream = GetFileStream(filename))
            {
                if (stream != null)
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    stream.Close();
                }

            }

            return data;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (package != null)
            {
                package.Dispose();
                package = null;
            }
        }

        #endregion

        public string Artist { get {
                try {
                    return Package.GetMetadata(MapMetaType.Artist);
                }
                catch { return "error"; }
        } }

        public string Creator { get { return Package.GetMetadata(MapMetaType.Creator); } }

        public string Title { get {
                try {
                    return Package.GetMetadata(MapMetaType.Title);
                }
                catch { return "error"; }
        } }

        public double DifficultyStars { get { return double.Parse(Package.GetMetadata(MapMetaType.DifficultyRating) ?? "0", GameBase.nfi); } }
    }
}

