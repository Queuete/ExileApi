using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using GameOffsets;
using JM.LinqFaster;

namespace ExileCore.PoEMemory.MemoryObjects
{
    public class EntityList : RemoteMemoryObject
    {
        private readonly WaitTime collectEntities = new WaitTime(1);
        private readonly List<long> hashAddresses = new List<long>(1000);
        private readonly HashSet<long> hashSet = new HashSet<long>(256);
        private readonly object locker = new object();
        private readonly Queue<long> queue = new Queue<long>(256);
        private readonly HashSet<long> StoreIds = new HashSet<long>(256);
        private readonly Stopwatch sw = Stopwatch.StartNew();
        public int EntitiesProcessed { get; private set; }

        public IEnumerator CollectEntities(EntityCollectSettingsContainer container)
        {
            if (Address == 0)
            {
                DebugWindow.LogError($"{nameof(EntityList)} -> Address is 0;");
                yield return new WaitTime(100);
            }

            while (!container.NeedUpdate)
            {
                yield return collectEntities;
            }

            sw.Restart();
            var dataEntitiesCount = container.EntitiesCount();
            var parseServerEntities = container.ParseServer();
            double jobsTimeSum = 0;
            var addr = M.Read<long>(Address + 0x8);
            hashAddresses.Clear();
            hashSet.Clear();
            StoreIds.Clear();
            queue.Enqueue(addr);
            var node = M.Read<EntityListOffsets>(addr);
            queue.Enqueue(node.FirstAddr);
            queue.Enqueue(node.SecondAddr);
            var loopcount = 0;

            while (queue.Count > 0 && loopcount < 10000)
            {
                try
                {
                    loopcount++;
                    var nextAddr = queue.Dequeue();

                    if (hashSet.Contains(nextAddr))
                        continue;

                    hashSet.Add(nextAddr);

                    if (nextAddr != addr && nextAddr != 0)
                    {
                        var entityAddress = node.Entity;

                        if (entityAddress > 0x100000000 && entityAddress < 0x7F0000000000)
                            hashAddresses.Add(entityAddress);

                        node = M.Read<EntityListOffsets>(nextAddr);
                        queue.Enqueue(node.FirstAddr);
                        queue.Enqueue(node.SecondAddr);
                    }
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"Entitylist while loop: {e}");
                }
            }

            EntitiesProcessed = hashAddresses.Count;

            if (dataEntitiesCount > 0 && EntitiesProcessed / dataEntitiesCount > 1.5f)
            {
                DebugWindow.LogError($"Something wrong we parse {EntitiesProcessed} when expect {dataEntitiesCount}");
                TheGame.IngameState.UpdateData();
            }

            if (container.ParseEntitiesInMultiThread() && container.CollectEntitiesInParallelWhenMoreThanX && container.MultiThreadManager != null &&
                EntitiesProcessed / container.MultiThreadManager.ThreadsCount >= 100)
            {
                var hashAddressesCount = hashAddresses.Count / container.MultiThreadManager.ThreadsCount;
                var jobs = new List<Job>(container.MultiThreadManager.ThreadsCount);
                var lastStart = container.MultiThreadManager.ThreadsCount * hashAddressesCount;

                for (var i = 1; i <= container.MultiThreadManager.ThreadsCount; i++)
                {
                    var i1 = i;

                    var job = container.MultiThreadManager.AddJob(() =>
                    {
                        try
                        {
                            int addressesCount;

                            if (i == container.MultiThreadManager.ThreadsCount)
                                addressesCount = hashAddresses.Count;
                            else
                                addressesCount = i1 * hashAddressesCount;

                            var start = (i1 - 1) * hashAddressesCount;
                            Span<uint> stackIds = stackalloc uint[addressesCount - start];
                            var index = 0;

                            for (var j = start; j < addressesCount; j++)
                            {
                                var addrEntity = hashAddresses[j];

                                stackIds[index] = ParseEntity(addrEntity, container.EntityCache, container.EntitiesVersion, container.Simple,
                                    parseServerEntities);

                                index++;
                            }

                            lock (locker)
                            {
                                foreach (var u in stackIds)
                                {
                                    StoreIds.Add(u);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            DebugWindow.LogError($"{e}");
                        }
                    }, $"EntityCollection {i}");

                    jobs.Add(job);
                }

                /*try
                {
                    Span<uint> stackIds = stackalloc uint[hashAddresses.Count-hashAddressesCount];
                    var index = 0;
                    for (var j = lastStart; j < hashAddresses.Count; j++)
                    {
                        var addrEntity = hashAddresses[j];
                       stackIds[index]= ParseEntity(addrEntity, entityCache, entitiesVersion, result);
                       index++;

                    }
                    lock (locker)
                    {
                        foreach (var u in stackIds)
                        {
                            StoreIds.Add(u);
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugWindow.LogError($"{e}");
                }*/

                while (!jobs.AllF(x => x.IsCompleted))
                {
                    sw.Stop();
                    yield return collectEntities;
                    sw.Start();
                }

                jobsTimeSum = jobs.SumF(x => x.ElapsedMs);
            }
            else
            {
                foreach (var addrEntity in hashAddresses)
                {
                    StoreIds.Add(ParseEntity(addrEntity, container.EntityCache, container.EntitiesVersion, container.Simple, parseServerEntities));
                }
            }

            if (container.Break)
            {
                container.Break = false;
                container.EntitiesVersion++;
                container.NeedUpdate = false;
                container.DebugInformation.Tick = sw.Elapsed.TotalMilliseconds;
                yield break;
            }

            foreach (var entity in container.EntityCache)
            {
                var entityValue = entity.Value;

                if (StoreIds.Contains(entity.Key))
                {
                    entityValue.IsValid = true;
                    continue;
                }

                entityValue.IsValid = false;

                var entityValueDistancePlayer = entityValue.DistancePlayer;

                if (entityValueDistancePlayer < 100)
                {
                    if (entityValueDistancePlayer < 75)
                    {
                        if (entityValue.Type == EntityType.Chest && entityValue.League == LeagueType.Delve)
                        {
                            if (entityValueDistancePlayer < 30)
                            {
                                container.KeyForDelete.Enqueue(entity.Key);
                                continue;
                            }
                        }
                        else
                        {
                            container.KeyForDelete.Enqueue(entity.Key);
                            continue;
                        }
                    }

                    if (entityValue.Type == EntityType.Monster && entityValue.IsAlive)
                    {
                        container.KeyForDelete.Enqueue(entity.Key);
                        continue;
                    }
                }

                if (entityValueDistancePlayer > 300 &&
                    entity.Value.Metadata.Equals("Metadata/Monsters/Totems/HeiTikiSextant", StringComparison.Ordinal))
                {
                    container.KeyForDelete.Enqueue(entity.Key);
                    continue;
                }

                if ((int) entityValue.Type < 100)
                {
                    container.KeyForDelete.Enqueue(entity.Key);
                    continue;
                }

                if (entityValueDistancePlayer > 1_000_000 || entity.Value.GridPos.IsZero)
                    container.KeyForDelete.Enqueue(entity.Key);
            }

            container.EntitiesVersion++;
            container.NeedUpdate = false;
            container.DebugInformation.Tick = sw.Elapsed.TotalMilliseconds + jobsTimeSum;
        }

        private uint ParseEntity(long addrEntity, IReadOnlyDictionary<uint, Entity> entityCache, uint entitiesVersion, Stack<Entity> result,
            bool parseServerEntities)
        {
            var entityId = M.Read<uint>(addrEntity + 0x60);
            if (entityId <= 0) return 0;

            if (entityId >= int.MaxValue && !parseServerEntities)
                return 0;

            if (entityCache.TryGetValue(entityId, out var Entity))
            {
                if (Entity.Address != addrEntity /*|| !Equals(Entity.EntityOffsets, ent)*/)
                {
                    Entity.UpdatePointer(addrEntity);

                    if (Entity.Check(entityId))
                    {
                        Entity.Version = entitiesVersion;
                        Entity.IsValid = true;
                    }
                }
                else
                {
                    Entity.Version = entitiesVersion;
                    Entity.IsValid = true;
                }
            }
            else
            {
                var entity = GetObject<Entity>(addrEntity);

                if (entity.Check(entityId))
                {
                    entity.Version = entitiesVersion;
                    result.Push(entity);
                    entity.IsValid = true;
                }
            }

            return entityId;
        }
    }
}
