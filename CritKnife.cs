using System;
using Oxide.Core.Plugins;
using UnityEngine;
using Rust;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Crit Knife", "somemando", "1.0.1")]
    [Description("This plugin allows creation of weapon that does a critical hit on scientists. Those with permissions can spawn the weapon, to sell in a shop or put in a customize vending machine.")]
    
    public class CritKnife : RustPlugin
    {
        //Weapon properties
        float critChance;
        int damageMultiplier;
        int maxCondition;
        int initialCondition;
        bool bloodEnabled;
        bool screamingEnabled;

        int weaponId = 2040726127;
        ulong skinId = 2943601828;

        //Effects
      
        string bloodEffectHead = "assets/bundled/prefabs/fx/player/bloodspurt_wounded_head.prefab";
        string bloodEffectGut = "assets/bundled/prefabs/fx/player/bloodspurt_wounded_stomache.prefab";
        string scream = "assets/bundled/prefabs/fx/player/gutshot_scream.prefab";
        string killNotify = "assets/bundled/prefabs/fx/kill_notify.prefab";

        private const string SpawnPermission = "critknife.spawn";
        private const string UsePermission = "critknife.use";

        private void Init()
        {
            permission.RegisterPermission(SpawnPermission, this);
            permission.RegisterPermission(UsePermission, this);

            LoadConfig();

            screamingEnabled = config.screamingEnabled;
            bloodEnabled = config.bloodEnabled;
            critChance = config.critChance;
            damageMultiplier = config.damageMultiplier;
            maxCondition = config.maxCondition;
            initialCondition = config.initialCondition;
        }

        void OnPluginLoaded(Plugin name)
        {
            Puts($"Plugin '{name}' has been loaded at {DateTime.Now.ToString()}");
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoSpawnPermission"] = "<color=red>You don't have permission to spawn a crit knife.</color>",
                ["KnifeName"] = "Critical Knife of Goldness"
            }, this);
        }

        private string GetLang(string langKey, string playerId = null, params object[] args)
        {
            return string.Format(lang.GetMessage(langKey, this, playerId), args);
        }

        // We detect if this is a scientist hit by a player with the right item and skin. The skin is only for the combat knife right now but this can change maybe?
        // If all that is the case, and if enabled, we then boost the damage and add effects.

        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            var attacker = hitInfo?.Initiator as BasePlayer;
            if (attacker == null)
            {
                return;
            }

            if ((attacker.UserIDString == null) || (attacker.IPlayer == null))
            {
                return;
            }

            if (!attacker.IPlayer.HasPermission(UsePermission))
            {
                return;
            }

            var hitPlayer = entity?.ToPlayer();
            if (hitPlayer == null)
            {
                return;
            }
            
            if (!entity.PrefabName.Contains("scientist")) 
            {
                return; 
            }

            var weapon = hitInfo.Weapon?.GetItem();
            if (weapon == null)
            {
                return;
            }
            if (weapon.info.itemid != weaponId)
            {
                return;
            }

            if (weapon.skin != skinId)
            {
                return;
            }
            
            hitInfo.damageTypes.Add(DamageType.ElectricShock, 10000f);
            HitInfo impact = new HitInfo()
            {
                HitPositionWorld = entity.transform.InverseTransformPoint(hitInfo.HitPositionWorld),
                HitNormalWorld = entity.transform.InverseTransformDirection(hitInfo.HitNormalWorld),
                HitMaterial = StringPool.Get("Flesh")
            };

            Effect.server.ImpactEffect(impact);

            if (screamingEnabled)
            {
                Effect.server.Run(scream, entity.transform.position, Vector3.zero, null, false);
            }

            if (bloodEnabled)
            {
                Effect.server.Run(bloodEffectHead, entity.transform.position, Vector3.zero, null, false);
                Effect.server.Run(bloodEffectGut, entity.transform.position, Vector3.zero, null, false);
            }

            Effect.server.Run(killNotify, entity.transform.position, Vector3.zero, null, false);

            entity.SendNetworkUpdateImmediate();

        }
                
        [ChatCommand("critknifespawn")]
        void cmdSpawnCritKnife(BasePlayer player, string command, string[] args)
        {
            if  (player.IPlayer.HasPermission(SpawnPermission)) 
            {
                var knife = ItemManager.CreateByItemID(weaponId, 1);
                knife.name = lang.GetMessage("KnifeName", this, player.UserIDString);
                knife.skin = skinId;
                var ent = knife.GetHeldEntity();
                ent.skinID = skinId;
                knife._maxCondition = maxCondition;
                knife._condition = initialCondition;
                knife.MarkDirty();
                player.inventory.GiveItem(knife);

            } else
            {
                SendReply(player, "<color=red>You don't have permission to spawn a crit knife.</color>");
            }
        }

        private PluginConfig config;

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<PluginConfig>();
            if (config == null)
            {
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(GetDefaultConfig(), true);
        }

        private PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                critChance = 1.0f,
                damageMultiplier = 10000,
                maxCondition = 500,
                initialCondition = 500,
                screamingEnabled = true,
                bloodEnabled = true,
            };
        }

    }

    public class PluginConfig
    {
        public float critChance;
        public int damageMultiplier;
        public int maxCondition;
        public int initialCondition;
        public bool screamingEnabled;
        public bool bloodEnabled;
    }
}
