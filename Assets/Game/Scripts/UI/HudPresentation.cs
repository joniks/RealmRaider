using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Scene-local skin and restrained feedback for runtime-created HUDs.</summary>
    [DisallowMultipleComponent]
    public sealed class HudPresentation : MonoBehaviour
    {
        const string ResourceRoot = "ThirdParty/Kenney/InterfacePolish/";
        Sprite buttonSprite;
        AudioClip click, confirm, result;
        AudioSource source;
        bool resultPlayed;
        bool ownsButtonSprite;
        bool initialized;

        public bool ResultCuePlayed => resultPlayed;

        void Awake() => EnsureInitialized();

        void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;
            buttonSprite = LoadButtonSprite();
            click = Resources.Load<AudioClip>(ResourceRoot + "Audio/click_001");
            confirm = Resources.Load<AudioClip>(ResourceRoot + "Audio/confirmation_003");
            result = Resources.Load<AudioClip>(ResourceRoot + "Audio/bong_001");
            source = GetComponent<AudioSource>();
            if (!source) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = .28f;
        }

        void OnDestroy()
        {
            if (ownsButtonSprite && buttonSprite) Destroy(buttonSprite);
        }

        Sprite LoadButtonSprite()
        {
            const string path = ResourceRoot + "Sprites/button_rectangle_depth_gradient";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite) return sprite;

            var texture = Resources.Load<Texture2D>(path);
            if (!texture) return null;
            ownsButtonSprite = true;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        public void ApplyButton(Image image)
        {
            EnsureInitialized();
            if (!image || !buttonSprite) return;
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
        }

        public void PlayClick() { EnsureInitialized(); Play(click); }
        public void PlayConfirm() { EnsureInitialized(); Play(confirm); }
        public void PlayResult()
        {
            EnsureInitialized();
            if (resultPlayed) return;
            resultPlayed = true;
            Play(result);
        }

        void Play(AudioClip clip)
        {
            if (clip && source) source.PlayOneShot(clip);
        }
    }
}
