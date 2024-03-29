﻿using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Mirror;
using NPCs.API.Components;
using NPCs.API.EventArgs;
using NPCs.API.Features.Objects;
using RemoteAdmin;
using UnityEngine;
using Map = NPCs.API.Features.Objects.Map;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace NPCs.API.Features
{
    public class Npc : Player
    {
        public static readonly HashSet<Npc> SpawnedNpc = new();

        public static readonly HashSet<Map> LoadedMaps = new();

        public new RoleType Role
        {
            get => ReferenceHub.characterClassManager.CurClass;
            set => ReferenceHub.characterClassManager.CurClass = value;
        }

        public new Vector3 Scale
        {
            get => ReferenceHub.transform.localScale;
            set => ReferenceHub.transform.localScale = value;
        }
        
        public RoomType Room { get; set; }

        private Npc(ReferenceHub referenceHub, string nickname, RoleType role, RoomType room, Vector3 position, Vector3 scale, Vector2 rotation, Item item = null) : base(referenceHub)
        {
            ReferenceHub.queryProcessor._ipAddress = "127.0.0.WAN";
            ReferenceHub.queryProcessor.NetworkPlayerId = QueryProcessor._idIterator++;

            ReferenceHub.characterClassManager.Start();
            ReferenceHub.playerStats.Start();
            ReferenceHub.nicknameSync.Start();
            ReferenceHub.playerMovementSync.Start();
            ReferenceHub.inventory.Start();
            ReferenceHub.serverRoles.Start();

            ReferenceHub.nicknameSync.MyNick = nickname;
            Scale = scale;
            Position = position;
            Rotation = rotation;
            Room = room;
            Role = role;

            ReferenceHub.characterClassManager.IsVerified = true;

            ReferenceHub.playerMovementSync.NetworkGrounded = true;
            IsGodModeEnabled = true;

            CurrentItem = item;
            
            SpawningNpcEventArgs ev = new SpawningNpcEventArgs(this);
            Handlers.Npc.OnSpawningNpc(ev);

            if (!ev.IsAllowed)
                return;

            NetworkServer.Spawn(GameObject);
            GameObject.AddComponent<Touching>();

            SpawnedNpc.Add(this);
            Dictionary.Add(GameObject, this);
        }
        
        public static Npc Spawn(string nickname, RoleType role, RoomType room, Vector3 position, Vector3 scale, Vector2 rotation)
            => new(ReferenceHub.GetHub(Object.Instantiate(NetworkManager.singleton.playerPrefab)), nickname, role, room, position, scale, rotation);

        public static Npc Spawn(Npc npc) => Spawn(npc.Nickname, npc.Role, npc.Room, npc.Position, npc.Scale, npc.Rotation);

        public static Npc Spawn(SerializableNpc npc) => Spawn(npc.Nickname, npc.Role, npc.Room, npc.Position, npc.Scale, npc.Rotation);

        public void LookAtPosition(Vector3 position)
        {
            Quaternion quaternion = Quaternion.LookRotation((position - Position).normalized);
            Rotation = new Vector2(quaternion.eulerAngles.x, quaternion.eulerAngles.y);
        }

        public void Update()
        {
            Destroy();
            Spawn(Nickname, Role, Room, Position, Scale, Rotation);
        }
        
        public void Destroy()
        {
            SpawnedNpc.Remove(this);
            Dictionary.Remove(GameObject);

            Object.Destroy(GameObject);
        }
    }
}