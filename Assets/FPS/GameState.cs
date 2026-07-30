using UnityEngine;
using System;

namespace Unity.FPS.Gameplay
{
    public struct GameEntityState : IEquatable<GameEntityState>
    {
        public int x;
        public int z;
        public int angle;
        public int health;

        public GameEntityState(Transform transform, float currentHP, float gridSize)
        {
            this.x = Mathf.FloorToInt(transform.localPosition.x / gridSize);
            this.z = Mathf.FloorToInt(transform.localPosition.z / gridSize);

            float rot = transform.eulerAngles.y;
            this.angle = Mathf.RoundToInt(rot / 45.0f) % 8;

            this.health = Bucketize(currentHP);
        }

        public GameEntityState(int x, int z, int angle, int health)
        {
            this.x = x;
            this.z = z;
            this.angle = angle;
            this.health = health;
        }

        // bucket condiviso: 0=dead/low, 1, 2, 3=high (usato sia da health che da ammoRatio)
        public static int Bucketize(float ratio)
        {
            if (ratio <= 0) return 0;
            if (ratio <= 0.3f) return 1;
            if (ratio <= 0.65f) return 2;
            return 3;
        }

        public string ToKey() => $"{x}_{z}_{angle}_{health}";

        public static GameEntityState Parse(string[] p, int offset = 0)
        {
            return new GameEntityState(
                int.Parse(p[offset]), int.Parse(p[offset + 1]), int.Parse(p[offset + 2]),
                int.Parse(p[offset + 3])
            );
        }

        public bool Equals(GameEntityState other) =>
            x == other.x && z == other.z &&
            angle == other.angle && health == other.health;

        public override bool Equals(object obj) => obj is GameEntityState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x.GetHashCode();
                hash = hash * 31 + z.GetHashCode();
                hash = hash * 31 + angle.GetHashCode();
                hash = hash * 31 + health.GetHashCode();
                return hash;
            }
        }
    }

    public struct PlayerState : IEquatable<PlayerState>
    {
        public GameEntityState entity;
        public int ammoRatio;

        public PlayerState(Transform transform, float ammoRatio, float currentHP, float gridSize)
        {
            this.entity = new GameEntityState(transform, currentHP, gridSize);
            this.ammoRatio = GameEntityState.Bucketize(ammoRatio);
        }

        public PlayerState(int x, int z, int angle, int ammoRatio, int health)
        {
            this.entity = new GameEntityState(x, z, angle, health);
            this.ammoRatio = ammoRatio;
        }

        public static PlayerState Parse(string s)
        {
            string[] p = s.Split('_');
            return new PlayerState(
                int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]),
                int.Parse(p[3]), int.Parse(p[4])
            );
        }

        public override string ToString() => $"{entity.ToKey()}_{ammoRatio}";

        public bool Equals(PlayerState other) =>
            entity.Equals(other.entity) && ammoRatio == other.ammoRatio;

        public override bool Equals(object obj) => obj is PlayerState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked { return entity.GetHashCode() * 31 + ammoRatio.GetHashCode(); }
        }
    }

    public struct EnemyState : IEquatable<EnemyState>
    {
        public GameEntityState entity;

        public EnemyState(Transform transform, float currentHP, float gridSize)
        {
            this.entity = new GameEntityState(transform, currentHP, gridSize);
        }

        public EnemyState(int x, int z, int angle, int health)
        {
            this.entity = new GameEntityState(x, z, angle, health);
        }

        public static EnemyState Parse(string s)
        {
            string[] p = s.Split('_');
            return new EnemyState(
                int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]),
                int.Parse(p[3])
            );
        }

        public override string ToString() => entity.ToKey();

        public bool Equals(EnemyState other) => entity.Equals(other.entity);

        public override bool Equals(object obj) => obj is EnemyState other && Equals(other);

        public override int GetHashCode() => entity.GetHashCode();
    }
    [System.Serializable]
    public struct GameState : IEquatable<GameState>
    {
        public PlayerState player;
        public EnemyState[] enemies;

        public GameState(PlayerState player, EnemyState[] enemies)
        {
            this.player = player;
            this.enemies = enemies;
        }

        /*
        public GameState(PlayerState player, Transform[] enemyTransforms, Rigidbody[] enemyRbs, float[] enemyHP, float gridSize)
        {
            this.player = player;
            this.enemies = new EnemyState[enemyTransforms.Length];
            for (int i = 0; i < enemyTransforms.Length; i++)
            {
                this.enemies[i] = new EnemyState(enemyTransforms[i], enemyRbs[i], enemyHP[i], gridSize);
            }
        }
        */

        public override string ToString()
        {
            // player|enemy0|enemy1|...
            string s = player.ToString();
            for (int i = 0; i < enemies.Length; i++)
                s += "|" + enemies[i].ToString();
            return s;
        }

        public static GameState Parse(string s, int enemyCount)
        {
            string[] parts = s.Split('|');
            
            PlayerState player = PlayerState.Parse(parts[0]);
            EnemyState[] enemies = new EnemyState[enemyCount];
            for (int i = 0; i < enemyCount; i++)
                enemies[i] = EnemyState.Parse(parts[i + 1]);

            return new GameState(player, enemies);
        }

        public bool Equals(GameState other)
        {
            if (!player.Equals(other.player)) return false;
            if (enemies.Length != other.enemies.Length) return false;

            for (int i = 0; i < enemies.Length; i++)
                if (!enemies[i].Equals(other.enemies[i])) return false;

            return true;
        }

        public override bool Equals(object obj) => obj is GameState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + player.GetHashCode();
                for (int i = 0; i < enemies.Length; i++)
                    hash = hash * 31 + enemies[i].GetHashCode();
                return hash;
            }
        }
    }
}