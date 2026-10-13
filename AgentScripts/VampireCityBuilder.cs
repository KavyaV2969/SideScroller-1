using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VampireCityBuilder
{
    private const string Town = "Assets/Art/Tilesets and Worlds/GothicVania Town/PNG/environment/";
    private const string Moon = "Assets/Art/Tilesets and Worlds/Moonlit Gothic Environment/";
    private static readonly Color Night = new Color(.67f, .76f, .98f, 1f);
    private static readonly Color Stone = new Color(.78f, .83f, 1f, 1f);
    private static readonly Color Distant = new Color(.43f, .56f, .86f, 1f);

    public static string Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Open SampleScene before building Vampire City.");
        if (GameObject.Find("Vampire City"))
            throw new InvalidOperationException("Vampire City already exists in the scene.");

        var grid = GameObject.Find("Grid");
        var originalBackdrop = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Background");
        if (!grid || !originalBackdrop) throw new InvalidOperationException("Demo terrain or backdrop is missing.");
        grid.SetActive(false);
        var oldParallax = originalBackdrop.GetComponent<ParallaxBackground>();
        if (oldParallax) UnityEngine.Object.DestroyImmediate(oldParallax);
        foreach (var renderer in originalBackdrop.GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = false;

        var root = new GameObject("Vampire City");
        var backdrop = Group(root.transform, "Parallax Backdrop");
        var sky = Group(backdrop, "Moonlit Sky");
        var skyline = Group(backdrop, "Distant City");
        var graveyard = Group(root.transform, "Graveyard Gate");
        var architecture = Group(root.transform, "Town Architecture");
        var streets = Group(root.transform, "Stone Streets");
        var details = Group(root.transform, "Street Details");
        var platforms = Group(root.transform, "Traversable Balconies");
        var collision = Group(root.transform, "Ground Colliders");

        var skySprite = Sprite(Moon + "Background_0.png");
        float skyWidth = skySprite.rect.width / skySprite.pixelsPerUnit * 4.167f;
        for (int i = -4; i <= 4; i++)
            PlaceCentered(sky, "Moonlit Clouds " + i, skySprite, i * skyWidth, -2.2f, 4.167f, 5.2f,
                "Background", 0, Color.white);

        var skylineSprite = Sprite(Town + "layers/middleground.png");
        float skylineWidth = skylineSprite.rect.width / skylineSprite.pixelsPerUnit * 5.21f;
        for (int i = -7; i <= 7; i++)
            PlaceBottom(skyline, "Gothic Skyline " + i, skylineSprite, i * skylineWidth, -5.8f,
                5.21f, 5.21f, "Hills", 0, Distant);

        var parallax = root.AddComponent<VampireCityParallax>();
        var serialized = new SerializedObject(parallax);
        serialized.FindProperty("sky").objectReferenceValue = sky;
        serialized.FindProperty("skyline").objectReferenceValue = skyline;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        // The cemetery is the quiet western approach to the inhabited city.
        var mausoleum = Sprite(Moon + "Background_1.png");
        foreach (float x in new[] { -75f, -55f })
            PlaceBottom(graveyard, "Moonlit Mausoleums", mausoleum, x, -5.2f, 2.9f, 2.9f,
                "Background Tiles", -3, Color.white);

        // Six adjoining street blocks make a continuous, roughly doubled route.
        var blocks = new[] {
            new Block(-90, -54, -5), new Block(-54, -20, -4),
            new Block(-20, 18, -4), new Block(18, 54, -5),
            new Block(54, 90, -4), new Block(90, 112, -5)
        };
        var cobble = Sprite(Town + "layers/sliced-tileset/ground.png");
        var cobbleAlt = Sprite(Town + "layers/sliced-tileset/ground-b.png");
        var wall = Sprite(Town + "layers/sliced-tileset/wall.png");
        int tileCount = 0;
        foreach (var block in blocks)
        {
            var fill = PlaceCentered(streets, "Deep Stonework " + block.Left, wall,
                (block.Left + block.Right) * .5f, block.Y - 9f,
                (block.Right - block.Left) * 100f / wall.rect.width, 18f * 100f / wall.rect.height,
                "Ground", -2, new Color(.23f, .20f, .32f, 1f));
            var colliderObject = new GameObject("Walkable Street " + block.Left + " to " + block.Right);
            colliderObject.transform.SetParent(collision, false);
            colliderObject.transform.position = new Vector3((block.Left + block.Right) * .5f, block.Y - 1.5f, 0);
            colliderObject.layer = 6;
            var box = colliderObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(block.Right - block.Left + .04f, 3f);
            for (int x = block.Left; x < block.Right; x++)
            {
                var sprite = ((x * 13) & 3) == 0 ? cobbleAlt : cobble;
                PlaceCentered(streets, "Cobble " + x, sprite, x + .5f, block.Y - 1.25f,
                    6.25f, 6.25f, "Ground", 0, Stone);
                tileCount++;
            }
        }

        // Houses are set behind gameplay actors so doors and windows remain legible.
        var houseA = Sprite(Town + "props-sliced/house-a.png");
        var houseB = Sprite(Town + "props-sliced/house-b.png");
        var houseC = Sprite(Town + "props-sliced/house-c.png");
        var houses = new (float x, int type)[] {
            (-45,0), (-36,2), (-27,1), (-17,0), (-8,2), (2,1), (11,0),
            (21,2), (31,1), (40,0), (49,2), (59,1), (68,0), (77,2), (85,1)
        };
        foreach (var house in houses)
        {
            var sprite = house.type == 0 ? houseA : house.type == 1 ? houseB : houseC;
            PlaceBottom(architecture, "Town House " + house.x, sprite, house.x,
                Surface(house.x) - .1f, 2.9f, 2.9f, "Background Tiles", 0, Night);
        }
        var church = Sprite(Town + "props-sliced/chuch.png");
        PlaceBottom(architecture, "Cathedral of the Crimson Moon", church, 101f,
            Surface(101) - .1f, 3.6f, 3.6f, "Background Tiles", 1, Stone);

        var lamp = Sprite(Town + "props-sliced/street-lamp.png");
        foreach (float x in new[] { -50f, -31f, -13f, 5f, 24f, 43f, 63f, 82f, 106f })
            PlaceBottom(details, "Gas Lamp " + x, lamp, x, Surface(x), 3.2f, 3.2f,
                "Background Tiles", 3, Stone);
        var wagon = Sprite(Town + "props-sliced/wagon.png");
        PlaceBottom(details, "Abandoned Wagon", wagon, 16f, Surface(16), 3.0f, 3.0f,
            "Background Tiles", 2, Night);
        var well = Sprite(Town + "props-sliced/well.png");
        PlaceBottom(details, "Town Well", well, 35f, Surface(35), 3.0f, 3.0f,
            "Background Tiles", 2, Night);
        var barrel = Sprite(Town + "props-sliced/barrel.png");
        var crate = Sprite(Town + "props-sliced/crate-stack.png");
        foreach (float x in new[] { -39f, -6f, 53f, 71f })
            PlaceBottom(details, "Barrel " + x, barrel, x, Surface(x), 3.1f, 3.1f,
                "Background Tiles", 4, Night);
        foreach (float x in new[] { -25f, 9f, 56f, 88f })
            PlaceBottom(details, "Crates " + x, crate, x, Surface(x), 2.8f, 2.8f,
                "Background Tiles", 4, Night);

        var wood = Sprite(Town + "layers/sliced-tileset/top-wood.png");
        AddBalcony(platforms, wood, -42, -36, -2.3f);
        AddBalcony(platforms, wood, -12, -5, -2.3f);
        AddBalcony(platforms, wood, 45, 52, -3.3f);
        AddBalcony(platforms, wood, 74, 81, -2.3f);

        // Preserve original serialized dialogue and combat setup by cloning the scene actors.
        Position(GameObject.Find("Player"), -73f, Surface(-73) + 1.36f);
        var knight = GameObject.Find("Enemy_Knight");
        Position(knight, -47f, Surface(-47) + 1.19f);
        Clone(knight, "Knight - Market Gate", 4f, Surface(4) + 1.19f);
        Clone(knight, "Knight - Well", 47f, Surface(47) + 1.19f);
        Clone(knight, "Knight - Cathedral Guard", 87f, Surface(87) + 1.19f);
        var lady = GameObject.Find("NPC_Lady");
        Position(lady, -16f, Surface(-16) + 1.33f);
        Clone(lady, "NPC Lady - Market", 33f, Surface(33) + 1.33f);
        var aiLady = GameObject.Find("AI_Lady");
        Position(aiLady, 65f, Surface(65) + 1.33f);
        var chest = GameObject.Find("Chest");
        Position(chest, 99f, Surface(99) + .48f);

        // Extend the existing Cinemachine confiner instead of changing camera wiring.
        var skyCollider = originalBackdrop.transform.Find("Background Sky")?.GetComponent<BoxCollider2D>();
        if (skyCollider)
        {
            var center = skyCollider.transform.InverseTransformPoint(new Vector3(11,-3,0));
            skyCollider.offset = new Vector2(center.x, center.y);
            skyCollider.size = new Vector2(210 / Mathf.Abs(skyCollider.transform.lossyScale.x),
                30 / Mathf.Abs(skyCollider.transform.lossyScale.y));
            var virtualCamera = GameObject.Find("CinemachineCamera");
            var confiner = virtualCamera ? virtualCamera.GetComponent<Unity.Cinemachine.CinemachineConfiner2D>() : null;
            if (confiner) confiner.InvalidateBoundingShapeCache();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Built Vampire City: " + tileCount + " stone street tiles, " + houses.Length +
               " houses, 4 balconies, 4 knights, 2 dialogue ladies, 1 AI lady; bounds x=-90..112.";
    }

    private struct Block
    {
        public int Left, Right, Y;
        public Block(int left, int right, int y) { Left = left; Right = right; Y = y; }
    }

    private static float Surface(float x)
    {
        if (x < -54) return -5;
        if (x < 18) return -4;
        if (x < 54) return -5;
        if (x < 90) return -4;
        return -5;
    }

    private static Transform Group(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Sprite Sprite(string path)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (!sprite) throw new InvalidOperationException("Missing sprite: " + path);
        return sprite;
    }

    private static GameObject PlaceCentered(Transform parent, string name, Sprite sprite,
        float x, float y, float scaleX, float scaleY, string layer, int order, Color tint)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(scaleX, scaleY, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = layer;
        renderer.sortingOrder = order;
        renderer.color = tint;
        return go;
    }

    private static GameObject PlaceBottom(Transform parent, string name, Sprite sprite,
        float x, float bottom, float scaleX, float scaleY, string layer, int order, Color tint)
    {
        float height = sprite.rect.height / sprite.pixelsPerUnit * scaleY;
        return PlaceCentered(parent, name, sprite, x, bottom + height * .5f,
            scaleX, scaleY, layer, order, tint);
    }

    private static void AddBalcony(Transform parent, Sprite wood, int left, int right, float top)
    {
        var group = Group(parent, "Balcony " + left);
        for (int x = left; x < right; x++)
            PlaceCentered(group, "Wood Plank " + x, wood, x + .5f, top - .44f,
                6.25f, 6.25f, "Ground", 2, Stone);
        var go = new GameObject("Walkable Balcony");
        go.transform.SetParent(group, false);
        go.transform.position = new Vector3((left + right) * .5f, top - .12f, 0);
        go.layer = 6;
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(right - left, .24f);
    }

    private static void Position(GameObject go, float x, float y)
    {
        if (!go) throw new InvalidOperationException("Missing original actor.");
        go.transform.position = new Vector3(x, y, go.transform.position.z);
    }

    private static GameObject Clone(GameObject original, string name, float x, float y)
    {
        var copy = UnityEngine.Object.Instantiate(original);
        copy.name = name;
        Position(copy, x, y);
        return copy;
    }
}
