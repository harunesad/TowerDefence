using System;
using UnityEngine;

namespace TowerDefence.Core.Security
{
    /// <summary>
    /// RAM tarayıcılarına (GameGuardian, Cheat Engine vb.) karşı değeri rastgele anahtarla
    /// maskeleyen ve bütünlük denetimi (canary/checksum) yapan güvenli tamsayı yapısı.
    /// </summary>
    [Serializable]
    public struct ObscuredInt : IEquatable<ObscuredInt>, IComparable<ObscuredInt>
    {
        private static int cryptoKeySalt = 0x5F3759DF;

        [SerializeField] private int currentCryptoKey;
        [SerializeField] private int hiddenValue;
        [SerializeField] private int fakeHoneyPot; // Taramalara yem olması için rastgele değişen fake değer
        [SerializeField] private int checksum;

        public ObscuredInt(int value)
        {
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(value, currentCryptoKey);
            fakeHoneyPot = unchecked(value + 1337);
            checksum = CalculateChecksum(value, currentCryptoKey);
        }

        private static int GenerateKey()
        {
            // Rastgele sıfırdan farklı bir XOR maskesi
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

        public int Get()
        {
            if (currentCryptoKey == 0)
            {
                // İlk boş oluşturulma durumu
                return 0;
            }

            int decrypted = Decrypt(hiddenValue, currentCryptoKey);
            int expectedChecksum = CalculateChecksum(decrypted, currentCryptoKey);

            if (checksum != expectedChecksum)
            {
                // Bellek tahrifatı tespit edildi!
                Debug.LogWarning("[Security] Memory tampering detected in ObscuredInt! Checksum mismatch.");
                return 0; // Hileyi sıfırla/etkisizleştir
            }

            return decrypted;
        }

        public void Set(int value)
        {
            // Değer her değiştiğinde anahtar rotasyonu yaparak RAM'deki adresi izlemeyi imkansız kıl
            currentCryptoKey = GenerateKey();
            hiddenValue = Encrypt(value, currentCryptoKey);
            fakeHoneyPot = unchecked(value + UnityEngine.Random.Range(100, 9999));
            checksum = CalculateChecksum(value, currentCryptoKey);
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

        public static ObscuredInt operator +(ObscuredInt a, int b) => new ObscuredInt(a.Get() + b);
        public static ObscuredInt operator -(ObscuredInt a, int b) => new ObscuredInt(a.Get() - b);
        public static ObscuredInt operator *(ObscuredInt a, int b) => new ObscuredInt(a.Get() * b);
        public static ObscuredInt operator /(ObscuredInt a, int b) => new ObscuredInt(a.Get() / b);

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
