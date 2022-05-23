using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExileCore.PoEMemory;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using GameOffsets.Native;
using ProcessMemoryUtilities.Memory;

namespace ExileCore
{
    public class Memory : IMemory
    {
        private bool closed;
        private readonly Stopwatch sw = Stopwatch.StartNew();

        public Memory((Process, Offsets) tuple)
        {
            Process = tuple.Item1;
            AddressOfProcess = Process.MainModule.BaseAddress.ToInt64();
            MainWindowHandle = Process.MainWindowHandle;

            OpenProcessHandle = ProcessMemory.OpenProcess(ProcessAccessFlags.VirtualMemoryRead, Process.Id);

            //  _openProcessHandle = WinApi.OpenProcess(Process, ProcessAccessFlags.All);
            //  SafeHandle = new SafeMemoryHandle(_openProcessHandle);
            // MagicReader = new MemoryLiterate(SafeHandle);
            BaseOffsets = tuple.Item2.DoPatternScans(this);
        }

        public IntPtr MainWindowHandle { get; }
        public IntPtr OpenProcessHandle { get; }

        //  public SafeMemoryHandle SafeHandle { get;  }

        //  public MemoryLiterate MagicReader { get;  }
        public long AddressOfProcess { get; }
        /*public bool AddressIsValid(IntPtr ptr) => AddressIsValid((ulong)ptr);
        public bool AddressIsValid(ulong value) => (value > 0x10000u && value < 0x000F000000000000u);*/
        public Dictionary<OffsetsName, long> BaseOffsets { get; }
        public Process Process { get; }

        public string ReadString(long addr, int length = 256, bool replaceNull = true)
        {
            if (addr <= 65536 && addr >= -1) return string.Empty;
            var @string = Encoding.ASCII.GetString(ReadMem(new IntPtr(addr), length));
            return replaceNull ? RTrimNull(@string) : @string;
        }

        public string ReadNativeString(long address)
        {
            var Size = Read<uint>(address + 0x8);
            var Reserved = Read<uint>(address + 0x10);

            //var size = Size;
            //if (size == 0)
            //    return string.Empty;
            if (Reserved == 0)
                return string.Empty;

            if ( /*8 <= size ||*/ 8 <= Reserved) //Have no idea how to deal with this
            {
                var readAddr = Read<long>(address);
                return ReadStringU(readAddr);
            }

            return ReadStringU(address);
        }

        public string ReadStringU(long addr, int length = 256, bool replaceNull = true)
        {
            if (length > 5120 || length < 0)
                return string.Empty;

            if (addr == 0)
                return string.Empty;

            var mem = ReadMem(new IntPtr(addr), length);

            if (mem.Length == 0)
                return string.Empty;

            if (mem[0] == 0 && mem[1] == 0)
                return string.Empty;

            var @string = Encoding.Unicode.GetString(mem);
            return replaceNull ? RTrimNull(@string) : @string;
        }

        public byte[] ReadMem(long addr, int size)
        {
            return ReadMem(new IntPtr(addr), size);
        }

        public byte[] ReadMem(IntPtr address, int size)
        {
            try
            {
                if (size <= 0 || address.ToInt64() <= 0 /*|| !AddressIsValid(address)*/) return new byte[0];
                var buffer = new byte[size];
                ProcessMemory.ReadProcessMemoryArray(OpenProcessHandle, address, buffer, 0, size);

                // NativeMethods.ReadProcessMemory(SafeHandle, address, buffer, size);
                return buffer;
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"Readmem-> A: {address} Size: {size}. {e}");
                throw;
            }
        }

        public byte[] ReadBytes(long addr, int size)
        {
            return ReadMem(addr, size);
        }

        public byte[] ReadBytes(long addr, long size)
        {
            return ReadMem(addr, (int)size);
        }

        public T[] ReadAsArray<T>(long addr, int size) where T : struct
        {
            if (size == 0)
                return Array.Empty<T>();
            T[] buffer = new T[size];
            try
            {
                ProcessMemory.ReadProcessMemoryArray(OpenProcessHandle, new IntPtr(addr), buffer, 0, size);
                return buffer;
            }
            catch (Exception e)
            {
                DebugWindow.LogError($"ReadAsArray-> A: {addr} Size: {size}. {e}");
                throw;
            }
        }
        public List<T> ReadStructsArray<T>(long startAddress, long endAddress, int structSize, RemoteMemoryObject game)
            where T : RemoteMemoryObject, new()
        {
            var result = new List<T>();
            var i = 0;
            var size = (endAddress - startAddress) / structSize;

            if (size < 0 || size > 100000)
            {
                DebugWindow.LogError($"Maybe overflow memory in {nameof(ReadStructsArray)} for reading structures of type: {typeof(T).Name}", 3);
                //throw new OverflowException($"Maybe overflow memory in {nameof(ReadStructsArray)} so much structs {size}");
                return result;
            }

            for (var address = startAddress; address < endAddress; address += structSize)
            {
                result.Add(game.GetObject<T>(address));
                i++;

                if (i > 100000)
                {
                    //throw new OverflowException($"Maybe overflow memory in {nameof(ReadStructsArray)} so much structs {size}");
                    DebugWindow.LogError($"Maybe overflow memory in {nameof(ReadStructsArray)} for reading structures of type: {typeof(T).Name}", 3);
                    return result;
                }
            }

            return result;
        }

        public IList<T> ReadDoublePtrVectorClasses<T>(long address, RemoteMemoryObject game, bool noNullPointers = false)
            where T : RemoteMemoryObject, new()
        {
            var start = Read<long>(address);

            //var end = ReadLong(address + 0x8);
            var last = Read<long>(address + 0x8);

            var length = (int)(last - start);
            var bytes = ReadMem(new IntPtr(start), length);
            var result = new List<T>();
            var sw = Stopwatch.StartNew();

            for (var readOffset = 0; readOffset < length; readOffset += 16)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    DebugWindow.LogError($"ReadDoublePtrVectorClasses error result count: {result.Count}");
                    return new List<T>();
                }

                var pointer = BitConverter.ToInt64(bytes, readOffset);

                if (pointer == 0 && noNullPointers)
                    continue;

                result.Add(game.GetObject<T>(pointer));
            }

            return result;
        }

        public IList<long> ReadPointersArray(long startAddress, long endAddress, int offset = 8)
        {
            if (startAddress <= 0 || endAddress <= 0) 
                return new List<long>();

            var length = endAddress - startAddress;

            if (length <= 0 || length > 20000 * 8)
                return new List<long>();

            sw.Restart();
            var result = new List<long>((int)(length / offset) + 1);
            var bytes = ReadMem(startAddress, (int)length);

            for (var i = 0; i < length; i += offset)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    DebugWindow.LogError($"ReadPointersArray error result count: {result.Count}");
                    return new List<long>();
                }

                result.Add(BitConverter.ToInt64(bytes, i));
            }

            return result;
        }

        public IList<long> ReadSecondPointerArray_Count(long startAddress, int count)
        {
            throw new NotImplementedException();
        }

        public T Read<T>(Pointer addr, params int[] offsets) where T : struct
        {
            throw new NotImplementedException();
        }

        public T Read<T>(IntPtr addr, params int[] offsets) where T : struct
        {
            if (addr == IntPtr.Zero) return default;
            var num = Read<long>(addr);
            var result = num;

            for (var index = 0; index < offsets.Length - 1; index++)
            {
                if (result == 0)
                    return default;

                var offset = offsets[index];
                result = Read<long>(result + offset);
            }

            if (result == 0)
                return default;

            return Read<T>(result + offsets[offsets.Length - 1]);
        }

        public T Read<T>(long addr, params int[] offsets) where T : struct
        {
            return Read<T>(new IntPtr(addr), offsets);
        }

        public T Read<T>(Pointer addr) where T : struct
        {
            throw new NotImplementedException();
        }

        public T Read<T>(IntPtr addr) where T : struct
        {
            if (addr == IntPtr.Zero /*|| !AddressIsValid(addr)*/)
            {
                // throw new Exception($"Invalid address ({addr}) for {typeof(T)}");
                return default;
            }

            var result = new T();
            ProcessMemory.ReadProcessMemory(OpenProcessHandle, addr, ref result);
            return result;
        }

        public T Read<T>(long addr) where T : struct
        {
            var ptr = new IntPtr(addr);
            /*if (!AddressIsValid(ptr))
            {
                return default;
            }*/
            return Read<T>(ptr);
        }

        public IList<Tuple<long, int>> ReadDoublePointerIntList(long address)
        {
            var result = new List<Tuple<long, int>>();
            var head = Read<long>(address);
            var node = Read<NativeListNode>(head);
            result.Add(new Tuple<long, int>(node.Ptr2_Key, node.Value));

            var sw = Stopwatch.StartNew();

            while (head != node.Next)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    DebugWindow.LogError($"ReadDoublePointerIntList error result count: {result.Count}");
                    return new List<Tuple<long, int>>();
                }

                node = Read<NativeListNode>(node.Next);
                result.Add(new Tuple<long, int>(node.Ptr2_Key, node.Value));
            }

            if (result.Count > 0)
                result.RemoveAt(result.Count - 1);

            return result;
        }

        public IList<T> ReadList<T>(IntPtr head) where T : struct
        {
            var result = new List<T>();

            var node = Read<NativeListNode>(head);

            var sw = Stopwatch.StartNew();
            var headLong = head.ToInt64();

            while (headLong != node.Next)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    DebugWindow.LogError($"Readlist error result count: {result.Count}");
                    return new List<T>();
                }

                result.Add(Read<T>(node.Next));
                node = Read<NativeListNode>(node.Next);
            }

            return result;
        }

        public IList<long> ReadListPointer(IntPtr head)
        {
            var result = new List<long>();

            var node = Read<NativeListNode>(head);

            var sw = Stopwatch.StartNew();
            var headLong = head.ToInt64();

            while (headLong != node.Next)
            {
                if (sw.ElapsedMilliseconds > 2000)
                {
                    DebugWindow.LogError($"ReadListPointer error result count: {result.Count}");
                    return new List<long>();
                }

                result.Add(node.Next);
                node = Read<NativeListNode>(node.Next);
            }

            return result;
        }

        public long[] FindPatterns(params IPattern[] patterns)
        {
            var exeImage = ReadMem(new IntPtr(AddressOfProcess), Process.MainModule.ModuleMemorySize); //33mb
            var address = new long[patterns.Length];

            bool CompareData(IPattern pattern, IReadOnlyList<byte> data, int offset)
            {
                if (pattern.Bytes[0] != data[offset] || pattern.Bytes[pattern.Bytes.Length - 1] != data[offset + pattern.Bytes.Length - 1])
                    return false;

                for (var i = 0; i < pattern.Bytes.Length; i++)
                {
                    if (pattern.Mask[i] == 'x' && pattern.Bytes[i] != data[offset + i])
                        return false;
                }

                return true;
            }

            void FindPattern(long iPattern)
            {
                var pattern = patterns[iPattern];
                var patternData = pattern.Bytes;
                var patternLength = patternData.Length;

                var found = false;

                using (new PerformanceTimer($"Pattern: {pattern.Name} -> ", 0,
                    (s, span) => DebugWindow.LogDebug(
                        $"{s}: Time: {span.TotalMilliseconds} ms. Offset:[{address[iPattern]}] Started searching offset with:{pattern.StartOffset}"),
                    false))
                {
                    for (var offset = pattern.StartOffset; offset < exeImage.Length - patternLength; offset++)
                    {
                        if (!CompareData(pattern, exeImage, offset)) continue;
                        found = true;
                        address[iPattern] = offset;
                        break;
                    }

                    if (found) return;

                    for (var offset = 0; offset < pattern.StartOffset; offset++)
                    {
                        if (!CompareData(pattern, exeImage, offset)) continue;
                        found = true;
                        address[iPattern] = offset;
                        break;
                    }
                }
            }

            var MultiThreading = true;

            //Little faster start hud
            //For me ~1400 vs ~800ms

            if (MultiThreading)
                Parallel.For((long)0, patterns.Length, FindPattern);
            else
            {
                for (var index = 0; index < patterns.Length; index++)
                {
                    FindPattern(index);
                }
            }

            exeImage = null;
            return address;
        }

        public bool IsInvalid()
        {
            return Process.HasExited || closed;
        }

        public IList<T> ReadNativeArray<T>(INativePtrArray ptrArray, int offset = 8) where T : struct
        {
            if (ptrArray.First == IntPtr.Zero)
                return new List<T>();

            var Length = (int)(ptrArray.Last.ToInt64() - ptrArray.First.ToInt64()) / offset;
            var result = new List<T>(Length);
            var pointers = ReadPointersArray(ptrArray.First.ToInt64(), ptrArray.Last.ToInt64(), offset);

            foreach (var pointer in pointers)
            {
                result.Add(Read<T>(pointer));
            }

            return result;
        }

        public void Dispose()
        {
            if (!closed)
            {
                closed = true;

                try
                {
                    ProcessMemory.CloseHandle(OpenProcessHandle);

                    //  SafeHandle.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"Error when dispose memory: {ex.Message}");
                }
            }
        }

        private static string RTrimNull(string text)
        {
            var num = text.IndexOf('\0');
            return num > 0 ? text.Substring(0, num) : text;
        }
    }
}
