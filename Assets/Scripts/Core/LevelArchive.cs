using System;using System.IO;using System.Linq;using System.Collections.Generic;using Newtonsoft.Json;
namespace Mewgic.Core
{
    public interface ILevelArchive{bool TryRead(int id,out GeneratedLevel level);void Write(GeneratedLevel level);}
    public sealed class MemoryLevelArchive:ILevelArchive
    {
        readonly Dictionary<int,GeneratedLevel> data=new Dictionary<int,GeneratedLevel>();
        public bool TryRead(int id,out GeneratedLevel level){lock(data){if(!data.TryGetValue(id,out var entry)){level=null;return false;}level=entry.Copy();return true;}}
        public void Write(GeneratedLevel level){level.Validate();lock(data){if(data.TryGetValue(level.level,out var old)){if(!LevelGenerator.Exact(old.Export(),level.Export()))throw new InvalidOperationException("Açılmış bölüm değiştirilemez.");return;}data[level.level]=level.Copy();}}
    }
    public sealed class FileLevelArchive:ILevelArchive
    {
        readonly string root;readonly object gate=new object();public FileLevelArchive(string directory){root=Path.GetFullPath(directory);}
        GeneratedLevel Read(string file){if(new FileInfo(file).Length>65536)throw new InvalidDataException("Bölüm belgesi çok büyük.");var level=JsonConvert.DeserializeObject<GeneratedLevel>(File.ReadAllText(file),new JsonSerializerSettings{MaxDepth=16,TypeNameHandling=TypeNameHandling.None});if(level==null)throw new InvalidDataException("Bölüm belgesi boş.");level.Validate();return level;}
        string Name(int id){if(id<3)throw new ArgumentOutOfRangeException(nameof(id));return Path.Combine(root,"level-"+id.ToString(System.Globalization.CultureInfo.InvariantCulture)+".json");}
        public bool TryRead(int id,out GeneratedLevel level)
        {
            lock(gate)
            {
                string file=Name(id);
                level=null;
                bool published=File.Exists(file)||File.Exists(file+".backup");
                var errors=new List<Exception>();
                foreach(string candidate in new[]{file,file+".backup",file+".tmp"})
                {
                    if(!File.Exists(candidate))continue;
                    try
                    {
                        var entry=Read(candidate);
                        if(entry.level!=id)throw new InvalidDataException("Bölüm kimliği uyuşmuyor.");
                        level=entry;
                        return true;
                    }
                    catch(Exception e) when(e is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException)
                    {errors.Add(e);}
                }
                if(!published||errors.Count==0)return false;
                throw new InvalidDataException("Açılmış bölüm kopyaları okunamadı; özgün kayıtlar korundu.",new AggregateException(errors));
            }
        }
        public void Write(GeneratedLevel level)
        {
            level.Validate();
            lock(gate)
            {
                if(TryRead(level.level,out var old))
                {
                    if(!LevelGenerator.Exact(old.Export(),level.Export()))throw new InvalidOperationException("Açılmış bölüm yeniden üretilemez.");
                    return;
                }
                Directory.CreateDirectory(root);
                string path=Name(level.level),temp=path+".tmp";
                if(File.Exists(temp))File.Move(temp,temp+".corrupt-"+Guid.NewGuid().ToString("N"));
                using(var file=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                {
                    using(var writer=new StreamWriter(file,System.Text.Encoding.UTF8,1024,true))
                        writer.Write(JsonConvert.SerializeObject(level));
                    file.Flush(true);
                }
                Read(temp);
                File.Move(temp,path);
                File.Copy(path,path+".backup",true);
            }
        }
    }

    public static class LevelArchiveHistory
    {
        public const int Window=64;
        public static string[] Recent(ILevelArchive archive,int before){var list=new List<string>();for(int offset=1;offset<=Window&&before-offset>=3;offset++)if(archive.TryRead(before-offset,out var level))list.Add(level.signature);return list.ToArray();}
    }
}
