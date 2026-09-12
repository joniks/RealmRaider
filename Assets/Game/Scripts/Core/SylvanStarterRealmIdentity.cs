using System;
using System.Globalization;
using RealmRaiders.Modules.StarterRealmLayouts;
using UnityEngine;

namespace RealmRaiders.Core
{
    public enum SylvanStarterRealmIdentityStatus
    {
        Failed,
        Loaded,
        Created,
        Recovered
    }

    public sealed class SylvanStarterRealmIdentityResult
    {
        internal SylvanStarterRealmIdentityResult(
            SylvanStarterRealmIdentityStatus status,
            string realmId,
            int seed,
            string layoutId,
            RealmLayoutRecipe recipe)
        {
            Status = status;
            RealmId = realmId;
            Seed = seed;
            LayoutId = layoutId;
            Recipe = recipe;
        }

        public SylvanStarterRealmIdentityStatus Status { get; }
        public string RealmId { get; }
        public int Seed { get; }
        public string LayoutId { get; }
        public RealmLayoutRecipe Recipe { get; }
        public bool HasIdentity => Status != SylvanStarterRealmIdentityStatus.Failed && Recipe != null;
    }

    /// <summary>
    /// Owns the one durable local identity for the future materialized Sylvan
    /// starter realm. Layout facts and deterministic selection remain module-owned.
    /// </summary>
    public static class SylvanStarterRealmIdentity
    {
        [Serializable]
        sealed class PersistedRecord
        {
            public int Version;
            public string RealmId;
            public int Seed;
            public string LayoutId;
        }

        const int CurrentVersion = 1;
        const string Key = "realmraiders.sylvanStarterRealmIdentity.v1";

        static Func<string> realmIdFactory = DefaultRealmId;
        static Func<int> seedFactory = DefaultSeed;
        static Func<int, string, RealmLayoutRecipe> selector = DefaultSelect;
        static Func<string, RealmLayoutRecipe> resolver = DefaultResolve;
        static Func<RealmLayoutRecipe, bool> validator = DefaultValidate;

        public static SylvanStarterRealmIdentityResult LoadOrCreate()
        {
            var hasRecord = PlayerPrefs.HasKey(Key);
            var json = PlayerPrefs.GetString(Key, string.Empty);
            if (!hasRecord)
                return Create(SylvanStarterRealmIdentityStatus.Created, false);

            if (TryResolve(json, out var loaded)) return loaded;
            return Create(SylvanStarterRealmIdentityStatus.Recovered, true);
        }

        static bool TryResolve(string json, out SylvanStarterRealmIdentityResult result)
        {
            result = Failed();
            if (!HasExactRecordShape(json)) return false;
            PersistedRecord record;
            try { record = JsonUtility.FromJson<PersistedRecord>(json); }
            catch { return false; }
            if (!IsStrictRecord(json, record)) return false;

            RealmLayoutRecipe recipe;
            try { recipe = resolver(record.LayoutId); }
            catch { return false; }
            if (!IsExactValidRecipe(recipe, record.LayoutId)) return false;

            result = Resolved(SylvanStarterRealmIdentityStatus.Loaded, record, recipe);
            return true;
        }

        static SylvanStarterRealmIdentityResult Create(SylvanStarterRealmIdentityStatus status, bool replacingInvalid)
        {
            try
            {
                var realmId = realmIdFactory();
                var seed = seedFactory();
                if (!IsCanonicalRealmId(realmId)) return FailCreation(replacingInvalid);

                var recipe = selector(seed, null);
                if (!IsExactValidRecipe(recipe, recipe?.LayoutId)) return FailCreation(replacingInvalid);

                var record = new PersistedRecord
                {
                    Version = CurrentVersion,
                    RealmId = realmId,
                    Seed = seed,
                    LayoutId = recipe.LayoutId
                };
                var json = JsonUtility.ToJson(record);
                if (!IsStrictRecord(json, record)) return FailCreation(replacingInvalid);

                PlayerPrefs.SetString(Key, json);
                PlayerPrefs.Save();
                return Resolved(status, record, recipe);
            }
            catch
            {
                return FailCreation(replacingInvalid);
            }
        }

        static SylvanStarterRealmIdentityResult FailCreation(bool replacingInvalid)
        {
            if (replacingInvalid && PlayerPrefs.HasKey(Key))
            {
                PlayerPrefs.DeleteKey(Key);
                PlayerPrefs.Save();
            }
            return Failed();
        }

        static bool IsStrictRecord(string json, PersistedRecord record)
        {
            if (record == null || record.Version != CurrentVersion || !IsCanonicalRealmId(record.RealmId) || string.IsNullOrEmpty(record.LayoutId))
                return false;
            return HasExactRecordShape(json);
        }

        static bool HasExactRecordShape(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            var index = 0;
            var fields = 0;
            var seenVersion = false;
            var seenRealmId = false;
            var seenSeed = false;
            var seenLayoutId = false;
            SkipWhitespace(json, ref index);
            if (!Consume(json, ref index, '{')) return false;

            while (true)
            {
                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == '}') break;
                if (!TryReadKey(json, ref index, out var key)) return false;
                SkipWhitespace(json, ref index);
                if (!Consume(json, ref index, ':')) return false;
                SkipWhitespace(json, ref index);

                switch (key)
                {
                    case "Version":
                        if (seenVersion || !TryReadInt32(json, ref index)) return false;
                        seenVersion = true;
                        break;
                    case "RealmId":
                        if (seenRealmId || !TryReadString(json, ref index)) return false;
                        seenRealmId = true;
                        break;
                    case "Seed":
                        if (seenSeed || !TryReadInt32(json, ref index)) return false;
                        seenSeed = true;
                        break;
                    case "LayoutId":
                        if (seenLayoutId || !TryReadString(json, ref index)) return false;
                        seenLayoutId = true;
                        break;
                    default:
                        return false;
                }

                fields++;
                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',')
                {
                    index++;
                    SkipWhitespace(json, ref index);
                    if (index >= json.Length || json[index] == '}') return false;
                    continue;
                }
                if (index < json.Length && json[index] == '}') break;
                return false;
            }

            if (!Consume(json, ref index, '}')) return false;
            SkipWhitespace(json, ref index);
            return index == json.Length && fields == 4 && seenVersion && seenRealmId && seenSeed && seenLayoutId;
        }

        static bool TryReadKey(string json, ref int index, out string key)
        {
            key = null;
            if (!Consume(json, ref index, '"')) return false;
            var start = index;
            while (index < json.Length && json[index] != '"')
            {
                if (json[index] == '\\' || json[index] < ' ') return false;
                index++;
            }
            if (index >= json.Length) return false;
            key = json.Substring(start, index - start);
            index++;
            return true;
        }

        static bool TryReadString(string json, ref int index)
        {
            if (!Consume(json, ref index, '"')) return false;
            while (index < json.Length)
            {
                var character = json[index++];
                if (character == '"') return true;
                if (character < ' ') return false;
                if (character != '\\') continue;
                if (index >= json.Length) return false;
                var escape = json[index++];
                if (escape == 'u')
                {
                    if (index + 4 > json.Length) return false;
                    for (var digit = 0; digit < 4; digit++) if (!IsHex(json[index + digit])) return false;
                    index += 4;
                }
                else if (escape != '"' && escape != '\\' && escape != '/' && escape != 'b' && escape != 'f' && escape != 'n' && escape != 'r' && escape != 't')
                    return false;
            }
            return false;
        }

        static bool TryReadInt32(string json, ref int index)
        {
            var start = index;
            if (index < json.Length && json[index] == '-') index++;
            var digits = index;
            while (index < json.Length && json[index] >= '0' && json[index] <= '9') index++;
            if (index == digits || index - digits > 1 && json[digits] == '0') return false;
            return int.TryParse(json.Substring(start, index - start), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _);
        }

        static bool IsHex(char value) => value >= '0' && value <= '9' || value >= 'a' && value <= 'f' || value >= 'A' && value <= 'F';

        static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && (json[index] == ' ' || json[index] == '\t' || json[index] == '\r' || json[index] == '\n')) index++;
        }

        static bool Consume(string json, ref int index, char expected)
        {
            if (index >= json.Length || json[index] != expected) return false;
            index++;
            return true;
        }

        static bool IsCanonicalRealmId(string value)
        {
            if (value == null || value.Length != 32) return false;
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                if (!(character >= '0' && character <= '9') && !(character >= 'a' && character <= 'f')) return false;
            }
            return Guid.TryParseExact(value, "N", out var parsed) && string.Equals(parsed.ToString("N"), value, StringComparison.Ordinal);
        }

        static bool IsExactValidRecipe(RealmLayoutRecipe recipe, string expectedLayoutId)
        {
            if (recipe == null || !string.Equals(recipe.LayoutId, expectedLayoutId, StringComparison.Ordinal)) return false;
            try { return validator(recipe); }
            catch { return false; }
        }

        static SylvanStarterRealmIdentityResult Resolved(
            SylvanStarterRealmIdentityStatus status,
            PersistedRecord record,
            RealmLayoutRecipe recipe) =>
            new(status, record.RealmId, record.Seed, record.LayoutId, recipe);

        static SylvanStarterRealmIdentityResult Failed() =>
            new(SylvanStarterRealmIdentityStatus.Failed, null, 0, null, null);

        static string DefaultRealmId() => Guid.NewGuid().ToString("N");
        static int DefaultSeed() => BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);

        static RealmLayoutRecipe DefaultSelect(int seed, string previousLayoutId)
        {
            var selected = StarterSylvanRealmLayoutSelector.Select(seed, previousLayoutId);
            return selected.Status == RealmLayoutSelectionStatus.Selected && selected.HasRecipe ? selected.Recipe : null;
        }

        static RealmLayoutRecipe DefaultResolve(string layoutId)
        {
            var resolved = StarterSylvanRealmLayoutResolver.ResolveExact(layoutId);
            return resolved.Status == RealmLayoutResolveStatus.Resolved && resolved.HasRecipe ? resolved.Recipe : null;
        }

        static bool DefaultValidate(RealmLayoutRecipe recipe) =>
            RealmLayoutRecipeValidator.ValidateStarterRecipe(recipe).IsValid;

        public static string KeyForTests => Key;

        public static void SetFactoriesForTests(Func<string> realmId, Func<int> seed)
        {
            realmIdFactory = realmId ?? DefaultRealmId;
            seedFactory = seed ?? DefaultSeed;
        }

        public static void ResetForTests()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            realmIdFactory = DefaultRealmId;
            seedFactory = DefaultSeed;
            selector = DefaultSelect;
            resolver = DefaultResolve;
            validator = DefaultValidate;
        }
    }
}
