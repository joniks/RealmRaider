using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class SylvanStarterRealmIdentityTests
    {
        const string RealmId = "00112233445566778899aabbccddeeff";
        const string RecoveryRealmId = "ffeeddccbbaa99887766554433221100";
        const string Ancient = "realmraiders.sylvan-layout.ancient-crossroads";
        const string Forked = "realmraiders.sylvan-layout.forked-canopy";
        const string Serpent = "realmraiders.sylvan-layout.serpent-roots";
        const string SentinelKey = "realmraiders.tests.starterRealm.sentinel";
        const string SentinelValue = "preserve-byte-for-byte:[]{}";

        bool hadIdentity;
        string previousIdentity;
        bool hadSentinel;
        string previousSentinel;

        [Serializable]
        sealed class TestRecord
        {
            public int Version;
            public string RealmId;
            public int Seed;
            public string LayoutId;
        }

        [SetUp]
        public void SetUp()
        {
            var key = SylvanStarterRealmIdentity.KeyForTests;
            hadIdentity = PlayerPrefs.HasKey(key);
            previousIdentity = PlayerPrefs.GetString(key, string.Empty);
            hadSentinel = PlayerPrefs.HasKey(SentinelKey);
            previousSentinel = PlayerPrefs.GetString(SentinelKey, string.Empty);
            SylvanStarterRealmIdentity.ResetForTests();
            PlayerPrefs.SetString(SentinelKey, SentinelValue);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            SylvanStarterRealmIdentity.ResetForTests();
            if (hadIdentity) PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, previousIdentity);
            if (hadSentinel) PlayerPrefs.SetString(SentinelKey, previousSentinel);
            else PlayerPrefs.DeleteKey(SentinelKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void MissingRecord_CreatesOnceThenLoadsExactCachedRecipeWithoutRewrite()
        {
            var realmCalls = 0;
            var seedCalls = 0;
            SylvanStarterRealmIdentity.SetFactoriesForTests(
                () => { realmCalls++; return RealmId; },
                () => { seedCalls++; return 4; });

            var created = SylvanStarterRealmIdentity.LoadOrCreate();
            var stored = PlayerPrefs.GetString(SylvanStarterRealmIdentity.KeyForTests);
            SetModuleDelegateToThrow("selector");
            var loaded = SylvanStarterRealmIdentity.LoadOrCreate();

            AssertResolved(created, SylvanStarterRealmIdentityStatus.Created, RealmId, 4, Forked);
            AssertResolved(loaded, SylvanStarterRealmIdentityStatus.Loaded, RealmId, 4, Forked);
            Assert.That(RecipeObject(loaded), Is.SameAs(RecipeObject(created)));
            Assert.That(realmCalls, Is.EqualTo(1));
            Assert.That(seedCalls, Is.EqualTo(1));
            Assert.That(PlayerPrefs.GetString(SylvanStarterRealmIdentity.KeyForTests), Is.EqualTo(stored), "A valid load must not rewrite its bytes.");
            Assert.That(PlayerPrefs.GetString(SentinelKey), Is.EqualTo(SentinelValue));
        }

        [TestCase(0, Ancient)]
        [TestCase(2, Serpent)]
        [TestCase(-2, Serpent)]
        public void SignedSeed_SelectsDeterministicExactPackageRecipe(int seed, string expectedLayoutId)
        {
            SylvanStarterRealmIdentity.SetFactoriesForTests(() => RealmId, () => seed);

            var first = SylvanStarterRealmIdentity.LoadOrCreate();
            var second = SylvanStarterRealmIdentity.LoadOrCreate();

            AssertResolved(first, SylvanStarterRealmIdentityStatus.Created, RealmId, seed, expectedLayoutId);
            AssertResolved(second, SylvanStarterRealmIdentityStatus.Loaded, RealmId, seed, expectedLayoutId);
            Assert.That(RecipeObject(second), Is.SameAs(RecipeObject(first)));
        }

        [Test]
        public void ValidRecord_ResolvesExactIdWithoutSelectorOrFactoryCalls()
        {
            var stored = JsonUtility.ToJson(new TestRecord { Version = 1, RealmId = RealmId, Seed = 0, LayoutId = Serpent }, true);
            PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, stored);
            PlayerPrefs.Save();
            SylvanStarterRealmIdentity.SetFactoriesForTests(
                () => throw new InvalidOperationException("Realm factory must not run."),
                () => throw new InvalidOperationException("Seed factory must not run."));
            SetModuleDelegateToThrow("selector");

            var loaded = SylvanStarterRealmIdentity.LoadOrCreate();

            AssertResolved(loaded, SylvanStarterRealmIdentityStatus.Loaded, RealmId, 0, Serpent);
            Assert.That(PlayerPrefs.GetString(SylvanStarterRealmIdentity.KeyForTests), Is.EqualTo(stored), "Valid formatting must remain byte-identical.");
        }

        [Test]
        public void InvalidJsonShapesAndExactIds_RecoverOnlyIsolatedCompleteRecord()
        {
            var invalidRecords = new[]
            {
                string.Empty,
                "null",
                "{not-json",
                Record(2, RealmId, 0, Ancient),
                Record(1, "deadbeef", 0, Ancient),
                Record(1, RealmId.ToUpperInvariant(), 0, Ancient),
                Record(1, RealmId, 0, "unknown-layout"),
                Record(1, RealmId, 0, Ancient.ToUpperInvariant()),
                Record(1, RealmId, 0, " " + Ancient),
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"LayoutId\":\"{Ancient}\"}}",
                Record(1, RealmId, 0, Ancient).TrimEnd('}') + ",\"Extra\":1}",
                Record(1, RealmId, 0, Ancient).TrimEnd('}') + ",\"Unexpected_Field\":1}",
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"Seed\":\"0\",\"LayoutId\":\"{Ancient}\"}}",
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"Seed\":null,\"LayoutId\":\"{Ancient}\"}}",
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"Seed\":2147483648,\"LayoutId\":\"{Ancient}\"}}",
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"Seed\":-2147483649,\"LayoutId\":\"{Ancient}\"}}",
                $"{{\"Version\":1,\"RealmId\":{{\"Value\":\"{RealmId}\"}},\"Seed\":0,\"LayoutId\":\"{Ancient}\"}}",
                $"{{\"Version\":1,\"RealmId\":\"{RealmId}\",\"Seed\":0,\"Seed\":0,\"LayoutId\":\"{Ancient}\"}}",
                Record(1, RealmId, 0, Ancient).TrimEnd('}') + ",}",
                Record(1, RealmId, 0, Ancient) + " trailing",
                "[" + Record(1, RealmId, 0, Ancient) + "]"
            };

            foreach (var invalid in invalidRecords)
            {
                SylvanStarterRealmIdentity.ResetForTests();
                PlayerPrefs.SetString(SentinelKey, SentinelValue);
                PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, invalid);
                PlayerPrefs.Save();
                SylvanStarterRealmIdentity.SetFactoriesForTests(() => RecoveryRealmId, () => 0);

                var recovered = SylvanStarterRealmIdentity.LoadOrCreate();

                AssertResolved(recovered, SylvanStarterRealmIdentityStatus.Recovered, RecoveryRealmId, 0, Ancient);
                Assert.That(PlayerPrefs.GetString(SentinelKey), Is.EqualTo(SentinelValue), invalid);
                Assert.That(PlayerPrefs.GetString(SylvanStarterRealmIdentity.KeyForTests), Is.EqualTo(Record(1, RecoveryRealmId, 0, Ancient)));
            }
        }

        [Test]
        public void ModuleSelectionResolutionAndValidationFailures_ReturnNoPartialIdentity()
        {
            SetModuleDelegateResult("selector", null);
            SylvanStarterRealmIdentity.SetFactoriesForTests(() => RealmId, () => 0);
            AssertFailedWithoutPartial(SylvanStarterRealmIdentity.LoadOrCreate());

            SylvanStarterRealmIdentity.ResetForTests();
            PlayerPrefs.SetString(SentinelKey, SentinelValue);
            PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, Record(1, RealmId, 0, Ancient));
            PlayerPrefs.Save();
            SylvanStarterRealmIdentity.SetFactoriesForTests(() => RecoveryRealmId, () => 0);
            SetModuleDelegateResult("resolver", null);
            SetModuleDelegateResult("selector", null);
            AssertFailedWithoutPartial(SylvanStarterRealmIdentity.LoadOrCreate());

            SylvanStarterRealmIdentity.ResetForTests();
            PlayerPrefs.SetString(SentinelKey, SentinelValue);
            PlayerPrefs.SetString(SylvanStarterRealmIdentity.KeyForTests, Record(1, RealmId, 0, Ancient));
            PlayerPrefs.Save();
            SylvanStarterRealmIdentity.SetFactoriesForTests(() => RecoveryRealmId, () => 0);
            SetModuleDelegateResult("validator", false);
            AssertFailedWithoutPartial(SylvanStarterRealmIdentity.LoadOrCreate());
        }

        [Test]
        public void EntropyFactoryFailures_ReturnNoPartialIdentity()
        {
            SylvanStarterRealmIdentity.SetFactoriesForTests(
                () => throw new InvalidOperationException("Realm ID entropy failed."),
                () => 0);
            AssertFailedWithoutPartial(SylvanStarterRealmIdentity.LoadOrCreate());

            SylvanStarterRealmIdentity.ResetForTests();
            PlayerPrefs.SetString(SentinelKey, SentinelValue);
            SylvanStarterRealmIdentity.SetFactoriesForTests(
                () => RealmId,
                () => throw new InvalidOperationException("Seed entropy failed."));
            AssertFailedWithoutPartial(SylvanStarterRealmIdentity.LoadOrCreate());
        }

        static void AssertResolved(
            SylvanStarterRealmIdentityResult result,
            SylvanStarterRealmIdentityStatus status,
            string realmId,
            int seed,
            string layoutId)
        {
            Assert.That(result.HasIdentity, Is.True);
            Assert.That(result.Status, Is.EqualTo(status));
            Assert.That(result.RealmId, Is.EqualTo(realmId));
            Assert.That(result.Seed, Is.EqualTo(seed));
            Assert.That(result.LayoutId, Is.EqualTo(layoutId));
            Assert.That(RecipeObject(result), Is.Not.Null);
        }

        static void AssertFailedWithoutPartial(SylvanStarterRealmIdentityResult result)
        {
            Assert.That(result.HasIdentity, Is.False);
            Assert.That(result.Status, Is.EqualTo(SylvanStarterRealmIdentityStatus.Failed));
            Assert.That(result.RealmId, Is.Null);
            Assert.That(result.LayoutId, Is.Null);
            Assert.That(RecipeObject(result), Is.Null);
            Assert.That(PlayerPrefs.HasKey(SylvanStarterRealmIdentity.KeyForTests), Is.False);
            Assert.That(PlayerPrefs.GetString(SentinelKey), Is.EqualTo(SentinelValue));
        }

        static object RecipeObject(SylvanStarterRealmIdentityResult result) =>
            typeof(SylvanStarterRealmIdentityResult).GetProperty("Recipe")?.GetValue(result);

        static string Record(int version, string realmId, int seed, string layoutId) =>
            JsonUtility.ToJson(new TestRecord { Version = version, RealmId = realmId, Seed = seed, LayoutId = layoutId });

        static void SetModuleDelegateToThrow(string fieldName)
        {
            var field = ModuleDelegateField(fieldName);
            var parameters = DelegateParameters(field.FieldType);
            var body = Expression.Throw(Expression.New(typeof(InvalidOperationException)), field.FieldType.GetMethod("Invoke").ReturnType);
            field.SetValue(null, Expression.Lambda(field.FieldType, body, parameters).Compile());
        }

        static void SetModuleDelegateResult(string fieldName, object value)
        {
            var field = ModuleDelegateField(fieldName);
            var returnType = field.FieldType.GetMethod("Invoke").ReturnType;
            var body = Expression.Constant(value, returnType);
            field.SetValue(null, Expression.Lambda(field.FieldType, body, DelegateParameters(field.FieldType)).Compile());
        }

        static FieldInfo ModuleDelegateField(string name) =>
            typeof(SylvanStarterRealmIdentity).GetField(name, BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(SylvanStarterRealmIdentity).FullName, name);

        static ParameterExpression[] DelegateParameters(Type delegateType) =>
            delegateType.GetMethod("Invoke").GetParameters().Select((parameter, index) => Expression.Parameter(parameter.ParameterType, "arg" + index)).ToArray();
    }
}
