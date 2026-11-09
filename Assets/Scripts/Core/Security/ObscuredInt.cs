using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence.Core.Security
{
    /// <summary>
    /// Değer şifreli durur. Anahtar struct'ın içinde değil, oturum sözlüğündedir.
    /// </summary>
    [Serializable]
    public struct ObscuredInt : IEquatable<ObscuredInt>, IComparable<ObscuredInt>
    {
        private static int cryptoKeySalt = 0x5F3759DF;
        private static int nextVaultId = 1;

        private struct VaultSlot
        {
            public int key;
            public int honeypot;
            public int committed;
        }

        private static readonly Dictionary<int, VaultSlot> keyVault = new Dictionary<int, VaultSlot>();

        [SerializeField] private int vaultId;
        [SerializeField] private int hiddenValue;
        [SerializeField] private int fakeHoneyPot;
        [SerializeField] private int checksum;

        public ObscuredInt(int value)
        {
            vaultId = 0;
            hiddenValue = 0;
            fakeHoneyPot = 0;
            checksum = 0;
            Set(value);
        }

        private static int GenerateKey()
        {
            int key = UnityEngine.Random.Range(100000, 2000000000);
            return key != 0 ? key : 123456789;
        }

        private static int Encrypt(int value, int key)
        {
            return value ^ key ^ cryptoKeySalt;
        }

        private static int Decrypt(int encrypted, int key)
        {
            return encrypted ^ key ^ cryptoKeySalt;
        }

        private static int CalculateChecksum(int value, int key)
        {
            return unchecked((value * 31) ^ (key * 17) ^ cryptoKeySalt);
        }

        private static int RentVault(int key, int honeypot, int committed)
        {
            int id = nextVaultId++;
            if (nextVaultId == int.MaxValue) nextVaultId = 1;
            keyVault[id] = new VaultSlot { key = key, honeypot = honeypot, committed = committed };
            return id;
        }

        public int Get()
        {
            if (vaultId == 0 || !keyVault.TryGetValue(vaultId, out VaultSlot slot))
                return 0;

            int decrypted = Decrypt(hiddenValue, slot.key);
            bool intact = checksum == CalculateChecksum(decrypted, slot.key) && fakeHoneyPot == slot.honeypot;
            if (!intact)
            {
                Debug.LogWarning("[Security] Memory tampering detected in ObscuredInt.");
                return slot.committed;
            }

            slot.committed = decrypted;
            keyVault[vaultId] = slot;
            return decrypted;
        }

        public void Set(int value)
        {
            int key = GenerateKey();
            int honey = UnityEngine.Random.Range(1, int.MaxValue);
            fakeHoneyPot = honey;
            vaultId = RentVault(key, honey, value);
            hiddenValue = Encrypt(value, key);
            checksum = CalculateChecksum(value, key);
        }

        public static implicit operator ObscuredInt(int value) => new ObscuredInt(value);
        public static implicit operator int(ObscuredInt obscured) => obscured.Get();

        public static ObscuredInt operator ++(ObscuredInt a)
        {
            a.Set(a.Get() + 1);
            return a;
        }

        public static ObscuredInt operator --(ObscuredInt a)
        {
            a.Set(a.Get() - 1);
            return a;
        }

        public static ObscuredInt operator +(ObscuredInt a, int b)
        {
            a.Set(a.Get() + b);
            return a;
        }

        public static ObscuredInt operator -(ObscuredInt a, int b)
        {
            a.Set(a.Get() - b);
            return a;
        }

        public static ObscuredInt operator *(ObscuredInt a, int b)
        {
            a.Set(a.Get() * b);
            return a;
        }

        public static ObscuredInt operator /(ObscuredInt a, int b)
        {
            a.Set(b == 0 ? a.Get() : a.Get() / b);
            return a;
        }

        public override string ToString() => Get().ToString();

        public override bool Equals(object obj)
        {
            if (obj is ObscuredInt other) return Get() == other.Get();
            if (obj is int intVal) return Get() == intVal;
            return false;
        }

        public bool Equals(ObscuredInt other) => Get() == other.Get();

        public int CompareTo(ObscuredInt other) => Get().CompareTo(other.Get());

        public override int GetHashCode() => Get().GetHashCode();
    }
}
