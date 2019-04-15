﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.SEAL;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SEALNetTest
{
    [TestClass]
    public class SmallModulusTests
    {
        [TestMethod]
        public void EmptyConstructorTest()
        {
            SmallModulus sm = new SmallModulus();

            Assert.IsNotNull(sm);
            Assert.IsTrue(sm.IsZero);
            Assert.AreEqual(0ul, sm.Value);
            Assert.AreEqual(0, sm.BitCount);
            Assert.AreEqual(1ul, sm.UInt64Count);
            Assert.IsFalse(sm.IsPrime);
        }

        [TestMethod]
        public void ValueConstructorTest()
        {
            SmallModulus sm = new SmallModulus(5);

            Assert.IsNotNull(sm);
            Assert.IsFalse(sm.IsZero);
            Assert.AreEqual(5ul, sm.Value);
            Assert.AreEqual(3, sm.BitCount);
            Assert.IsTrue(sm.IsPrime);

            // Value is exactly 62 bits
            SmallModulus sm2 = new SmallModulus(0x3FFFFFFFFFFFFFFFul);

            Assert.IsNotNull(sm2);
            Assert.IsFalse(sm2.IsZero);
            Assert.AreEqual(0x3FFFFFFFFFFFFFFFul, sm2.Value);
            Assert.AreEqual(62, sm2.BitCount);
            Assert.AreEqual(1ul, sm2.UInt64Count);
            Assert.IsFalse(sm2.IsPrime);

            SmallModulus sm3 = new SmallModulus(0xF00000F000079ul);

            Assert.IsNotNull(sm3);
            Assert.IsFalse(sm3.IsZero);
            Assert.AreEqual(0xF00000F000079ul, sm3.Value);
            Assert.AreEqual(52, sm3.BitCount);
            Assert.AreEqual(1ul, sm3.UInt64Count);
            Assert.IsTrue(sm3.IsPrime);
        }

        [TestMethod]
        public void CopyConstructorTest()
        {
            SmallModulus sm = new SmallModulus(10);
            SmallModulus copy = new SmallModulus(sm);

            Assert.IsNotNull(copy);
            Assert.IsFalse(copy.IsZero);
            Assert.AreEqual(10ul, copy.Value);
        }

        [TestMethod]
        public void ExplicitCreateTest()
        {
            SmallModulus sm = (SmallModulus)34ul;
            Assert.IsNotNull(sm);
            Assert.AreEqual(34ul, sm.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFail1Test()
        {
            // Should fail if value is 1
            SmallModulus sm = new SmallModulus(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFail2Test()
        {
            // Should fail if value is larger than 62 bits
            SmallModulus sm = new SmallModulus(0x7FFFFFFFFFFFFFFFul);
        }

        [TestMethod]
        public void SetTest()
        {
            SmallModulus sm1 = new SmallModulus(456);
            SmallModulus sm2 = new SmallModulus();

            Assert.IsFalse(sm1.IsZero);
            Assert.AreEqual(456ul, sm1.Value);
            Assert.IsFalse(sm1.IsPrime);
            Assert.IsTrue(sm2.IsZero);
            Assert.AreEqual(0ul, sm2.Value);
            Assert.IsFalse(sm2.IsPrime);

            sm2.Set(sm1);

            Assert.IsFalse(sm2.IsZero);
            Assert.AreEqual(456ul, sm2.Value);
            Assert.IsFalse(sm2.IsPrime);

            sm2.Set(value: 65537ul);

            Assert.IsFalse(sm2.IsZero);
            Assert.AreEqual(65537ul, sm2.Value);
            Assert.IsTrue(sm2.IsPrime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetFail1Test()
        {
            // Should faile if set to 1
            SmallModulus sm = new SmallModulus();
            sm.Set(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetFail2Test()
        {
            // Should fail if set to bigger than 62 bits
            SmallModulus sm = new SmallModulus();
            sm.Set(0x7FFFFFFFFFFFFFFFul);
        }

        [TestMethod]
        public void ConstRatioTest()
        {
            SmallModulus sm = new SmallModulus();
            sm.Set(0x1234567890ABCDEFul);

            Tuple<ulong, ulong, ulong> ratio = sm.ConstRatio;

            Assert.IsNotNull(ratio);
            Assert.AreNotEqual(0ul, ratio.Item1);
            Assert.AreNotEqual(0ul, ratio.Item2);
            Assert.AreNotEqual(0ul, ratio.Item3);

            sm.Set(0xF00000F000079ul);
            ratio = sm.ConstRatio;

            Assert.IsNotNull(ratio);
            Assert.AreEqual(1224979096621368355ul, ratio.Item1);
            Assert.AreEqual(4369ul, ratio.Item2);
            Assert.AreEqual(1144844808538997ul, ratio.Item3);
        }

        [TestMethod]
        public void EqualsTest()
        {
            SmallModulus sm1 = new SmallModulus(0x12345ul);
            SmallModulus sm2 = new SmallModulus(0x12345ul);

            Assert.AreEqual(sm1, sm2);
            Assert.AreEqual(sm1.GetHashCode(), sm2.GetHashCode());
            Assert.IsTrue(sm1.Equals(0x12345ul));
            Assert.IsFalse(sm1.Equals(0x1234ul));

            Assert.IsFalse(sm1.Equals(null));
        }

        [TestMethod]
        public void SaveLoadTest()
        {
            SmallModulus sm1 = new SmallModulus(65537ul);
            SmallModulus sm2 = new SmallModulus();

            Assert.AreNotSame(sm1, sm2);
            Assert.AreNotEqual(sm1, sm2);
            Assert.AreNotEqual(sm1.IsPrime, sm2.IsPrime);


            using (MemoryStream stream = new MemoryStream())
            {
                sm1.Save(stream);

                stream.Seek(offset: 0, loc: SeekOrigin.Begin);

                sm2.Load(stream);
            }

            Assert.AreNotSame(sm1, sm2);
            Assert.AreEqual(sm1, sm2);
            Assert.AreEqual(sm1.BitCount, sm2.BitCount);
            Assert.AreEqual(sm1.UInt64Count, sm2.UInt64Count);
            Assert.AreEqual(sm1.ConstRatio.Item1, sm2.ConstRatio.Item1);
            Assert.AreEqual(sm1.ConstRatio.Item2, sm2.ConstRatio.Item2);
            Assert.AreEqual(sm1.ConstRatio.Item3, sm2.ConstRatio.Item3);
            Assert.AreEqual(sm1.IsPrime, sm2.IsPrime);
        }

        [TestMethod]
        public void GeneratePrimesTest()
        {
            SmallModulus sm = DefaultParams.SmallMods30Bit(0);
            Assert.IsTrue(sm.IsPrime);
            sm.Set(DefaultParams.SmallMods30Bit(0).Value * DefaultParams.SmallMods30Bit(1).Value);
            Assert.IsFalse(sm.IsPrime);

            List<SmallModulus> primes = (List<SmallModulus>)SmallModulus.GetPrimes(50, 8, 1 << 16);

            Assert.AreEqual(primes.Count, 8);
            for (int i = 0; i < primes.Count; i++)
            {
                if (i != 0)
                {
                    Assert.IsTrue(primes[i].Value < primes[i - 1].Value);
                }
                Assert.AreEqual(primes[i].BitCount, 50);
                Assert.IsTrue(primes[i].IsPrime);
            }
        }

        [TestMethod]
        public void ExceptionsTest()
        {
            SmallModulus sm = new SmallModulus(0x12345ul);
            MemoryStream ms_empty = new MemoryStream();

            Assert.ThrowsException<ArgumentNullException>(() => sm = new SmallModulus(null));

            Assert.ThrowsException<ArgumentNullException>(() => sm.Set(null));

            Assert.ThrowsException<ArgumentNullException>(() => sm.Save(null));
            Assert.ThrowsException<ArgumentNullException>(() => sm.Load(null));
            Assert.ThrowsException<ArgumentException>(() => sm.Load(ms_empty));

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Bit count
                writer.Write(16);
                // Uint64 count
                writer.Write(1ul);
                // Value that does not match bit count
                writer.Write(0x123456789ABCDul);
            }

            ms.Seek(offset: 0, loc: SeekOrigin.Begin);
            Assert.ThrowsException<InvalidOperationException>(() => sm.Load(ms));

            ms = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Bit count
                writer.Write(16);
                // UInt64 count that does not match value
                writer.Write(2ul);
                // Value
                writer.Write(0xFABCul);
            }

            ms.Seek(offset: 0, loc: SeekOrigin.Begin);
            Assert.ThrowsException<InvalidOperationException>(() => sm.Load(ms));

            IEnumerable<SmallModulus> primes = null;
            Assert.ThrowsException<ArgumentException>(() => primes = SmallModulus.GetPrimes(-1, 1, 1));
            Assert.ThrowsException<ArgumentException>(() => primes = SmallModulus.GetPrimes(0, 1, 1));
            primes = SmallModulus.GetPrimes(2, 1, 1);
            Assert.ThrowsException<ArgumentException>(() => primes = SmallModulus.GetPrimes(30, 0, 1));
            Assert.ThrowsException<ArgumentException>(() => primes = SmallModulus.GetPrimes(30, 1, 0));
            primes = SmallModulus.GetPrimes(30, 1, 1);
            Assert.ThrowsException<InvalidOperationException>(() => primes = SmallModulus.GetPrimes(5, 10, 1));
            primes = SmallModulus.GetPrimes(50, 10, 1);
            Assert.AreEqual(((List<SmallModulus>)primes).Count, 10);
        }
    }
}
