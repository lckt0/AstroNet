using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;

namespace KVM
{
    internal class NativeEraser
    {
        private static void Erase(Tuple<uint, uint, byte[]> section, uint offset, uint len)
        {
            Array.Clear(section.Item3, (int)(offset - section.Item1), (int)len);
        }

        private static void Erase(List<Tuple<uint, uint, byte[]>> sections, uint beginOffset, uint size)
        {
            foreach (Tuple<uint, uint, byte[]> sect in sections)
            {
                if (beginOffset >= sect.Item1 && beginOffset + size < sect.Item2)
                {
                    Erase(sect, beginOffset, size);
                    break;
                }
            }
        }

        private static void Erase(List<Tuple<uint, uint, byte[]>> sections, IFileSection s)
        {
            foreach (Tuple<uint, uint, byte[]> sect in sections)
            {
                if ((uint)s.StartOffset >= sect.Item1 && (uint)s.EndOffset < sect.Item2)
                {
                    Erase(sect, (uint)s.StartOffset, (uint)(s.EndOffset - s.StartOffset));
                    break;
                }
            }
        }

        private static void Erase(List<Tuple<uint, uint, byte[]>> sections, uint methodOffset)
        {
            foreach (Tuple<uint, uint, byte[]> sect in sections)
            {
                if (methodOffset >= sect.Item1)
                {
                    uint f = (uint)sect.Item3[(int)((UIntPtr)(methodOffset - sect.Item1))];
                    uint size;
                    switch (f & 7u)
                    {
                        case 2u:
                        case 6u:
                            size = (f >> 2) + 1u;
                            break;
                        case 3u:
                            {
                                f |= (uint)((uint)sect.Item3[(int)((UIntPtr)(methodOffset - sect.Item1 + 1u))] << 8);
                                size = (f >> 12) * 4u;
                                uint codeSize = BitConverter.ToUInt32(sect.Item3, (int)(methodOffset - sect.Item1 + 4u));
                                size += codeSize;
                                break;
                            }
                        case 4u:
                        case 5u:
                            goto IL_98;
                        default:
                            goto IL_98;
                    }
                    Erase(sect, methodOffset, size);
                    continue;
                IL_98:
                    break;
                }
            }
        }

        public static void Erase(NativeModuleWriter writer, ModuleDefMD module)
        {

            if (writer == null || module == null)
            {
                return;
            }
            List<Tuple<uint, uint, byte[]>> sections = new List<Tuple<uint, uint, byte[]>>();
            MemoryStream s = new MemoryStream();
            foreach (NativeModuleWriter.OrigSection origSect in writer.OrigSections)
            {
                var oldChunk = origSect.Chunk;
                ImageSectionHeader sectHdr = origSect.PESection;
                s.SetLength(0L);
                oldChunk.WriteTo(  new BinaryWriter(s));
                byte[] buf = s.ToArray();
                var newChunk =  new BinaryReaderChunk(MemoryImageStream.Create(buf), oldChunk.GetVirtualSize());
                newChunk.SetOffset(oldChunk.FileOffset, oldChunk.RVA);
                origSect.Chunk = newChunk;
                sections.Add(Tuple.Create<uint, uint, byte[]>(sectHdr.PointerToRawData, sectHdr.PointerToRawData + sectHdr.SizeOfRawData, buf));
            }

            var md = module.MetaData;
            uint row = md.TablesStream.MethodTable.Rows;
            for (uint i = 1u; i <= row; i += 1u)
            {
                RawMethodRow method = md.TablesStream.ReadMethodRow(i);

                if ((method.ImplFlags & 3) == 0)
                {
                    Erase(sections, (uint)md.PEImage.ToFileOffset((RVA)method.RVA));
                }
            }
            ImageDataDirectory res = md.ImageCor20Header.Resources;
            if (res.Size > 0u)
            {
                Erase(sections, (uint)res.StartOffset, res.Size);
            }
            Erase(sections, md.ImageCor20Header);
            Erase(sections, md.MetaDataHeader);// md.MetadataHeader);
            foreach (DotNetStream stream in md.AllStreams)
            {
                Erase(sections, stream);
            }
        }
        
    }
}
