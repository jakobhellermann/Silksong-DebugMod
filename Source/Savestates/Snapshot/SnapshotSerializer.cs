using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using DebugMod.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.UnityConverters.Math;
using TMProOld;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace DebugMod.Savestates.Snapshot;

public static class SnapshotSerializer {
    public static void SnapshotRecursive(
        Component component,
        List<ComponentSnapshot> snapshots,
        HashSet<Component> seen,
        int? maxDepth = null
    ) {
        SnapshotRecursive(component, snapshots, seen, component, 0, maxDepth);
    }

    private static void SnapshotRecursive(
        Component component,
        List<ComponentSnapshot> snapshots,
        HashSet<Component> seen,
        Component? onlyDescendantsOf,
        int depth,
        int? maxDepth = null
    ) {
        if (seen.Contains(component)) return;

        RefConverter.References.Clear();
        var tok = JToken.FromObject(component, JsonSerializer.Create(Settings));

        seen.Add(component);
        snapshots.Add(new ComponentSnapshot {
            Data = tok,
            Path = ObjectUtils.ObjectComponentPath(component),
        });

        if (depth >= maxDepth) {
            return;
        }

        foreach (var reference in RefConverter.References.ToArray()) {
            var recurseIntoReference = !onlyDescendantsOf || reference.transform.IsChildOf(component.transform);
            if (recurseIntoReference) {
                SnapshotRecursive(reference, snapshots, seen, onlyDescendantsOf, depth, maxDepth);
            }
        }
    }

    public static JToken Snapshot(object obj) => JToken.FromObject(obj, JsonSerializer.Create(Settings));

    public static string SnapshotToString(object? obj) =>
        JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);

    public static void Populate(object target, string json) {
        using JsonReader reader = new JsonTextReader(new StringReader(json));
        JsonSerializer.Create(Settings).Populate(reader, target);
    }

    public static void Populate(object target, JToken json) {
        var serializer = JsonSerializer.Create(Settings);
        using JsonReader reader = new JTokenReader(json);
        serializer.Populate(reader, target);
    }

    private static readonly JsonSerializerSettings Settings = new() {
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        Error = (_, args) => {
            args.ErrorContext.Handled = true;
            Log.Error(
                $"Serialization during snapshot: {args.CurrentObject?.GetType()}: {args.ErrorContext.Path}: {args.ErrorContext.Error.Message}");
        },
        ContractResolver = resolver,
        Converters = new List<JsonConverter> {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new QuaternionConverter(),
            new ColorConverter(),
            new Color32Converter(),
            // new AnimatorConverter(), TODO
            new StringEnumConverter(),
        },
    };

    private static CustomizableContractResolver resolver => new() {
        ContainerTypesToIgnore = [
            typeof(MonoBehaviour),
            typeof(Component),
            typeof(Object),
        ],
        FieldTypesToIgnore = [
            // ignored
            typeof(Camera),
            typeof(GameObject),
            typeof(UnityEventBase),
            typeof(Action),
            typeof(Delegate),
            typeof(PositionConstraint),
            typeof(TextMeshProUGUI),
            typeof(TMP_Text),
            typeof(Sprite),
            typeof(Tilemap),
            typeof(LineRenderer),
            typeof(Color),
            typeof(ParticleSystem),
            typeof(AnimationCurve),
            typeof(AnimationClip),
            typeof(Rect),
            // todo
            typeof(Transform), // maybe
            typeof(RenderTexture),
            typeof(Texture2D),
            typeof(Texture3D),
            typeof(SpriteRenderer), // maybe
            typeof(LayerMask), // maybe
            typeof(Collider2D), // maybe
            typeof(ScriptableObject),
        ],
        ExactFieldTypesToIgnore = [typeof(Component)],
        FieldAllowlist = new Dictionary<Type, string[]> {
            { typeof(Transform), ["localPosition", "localRotation", "localScale"] },
            { typeof(Rigidbody2D), ["position", "linearVelocity"] }, {
                typeof(HeroController), [
                    "cState",
                ]
            },
        },
        FieldDenylist = new Dictionary<Type, string[]>(),
    };

    internal static void RemoveNullFields(JToken token, params string[] fields) {
        if (token is not JContainer container) return;

        var removeList = new List<JToken>();
        foreach (var el in container.Children()) {
            if (el is JProperty p && fields.Contains(p.Name) && p.Value.ToObject<object>() == null) {
                removeList.Add(el);
            }

            RemoveNullFields(el, fields);
        }

        foreach (var el in removeList) {
            el.Remove();
        }
    }
}