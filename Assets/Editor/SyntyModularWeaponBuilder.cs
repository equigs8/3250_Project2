using UnityEngine;
using UnityEditor;
using InfimaGames.LowPolyShooterPack;

public class SyntyModularWeaponBuilder : EditorWindow
{
    private string weaponName = "New Modular Weapon";

    // Modular Parts
    private GameObject bodyPrefab;
    private GameObject barrelPrefab;
    private GameObject stockPrefab;
    private GameObject gripPrefab;
    private GameObject handguardPrefab;
    private GameObject magazinePrefab;
    private GameObject scopePrefab;
    private GameObject attachmentPrefab; // For lasers, flashlights, etc.

    [MenuItem("Tools/Synty Modular Weapon Builder")]
    public static void ShowWindow()
    {
        GetWindow<SyntyModularWeaponBuilder>("Modular Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Synty Modular Weapon Builder (LPSP)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag your Synty Modular prefabs into the slots below. The tool will assemble them and automatically apply the Infima LPSP hierarchy and scripts.", MessageType.Info);

        GUILayout.Space(10);
        
        weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);

        GUILayout.Space(10);
        GUILayout.Label("Core Modules", EditorStyles.boldLabel);
        bodyPrefab = (GameObject)EditorGUILayout.ObjectField("Body (Required)", bodyPrefab, typeof(GameObject), false);
        barrelPrefab = (GameObject)EditorGUILayout.ObjectField("Barrel", barrelPrefab, typeof(GameObject), false);
        stockPrefab = (GameObject)EditorGUILayout.ObjectField("Stock", stockPrefab, typeof(GameObject), false);
        gripPrefab = (GameObject)EditorGUILayout.ObjectField("Grip", gripPrefab, typeof(GameObject), false);
        handguardPrefab = (GameObject)EditorGUILayout.ObjectField("Handguard", handguardPrefab, typeof(GameObject), false);

        GUILayout.Space(10);
        GUILayout.Label("Functional Modules (LPSP Linked)", EditorStyles.boldLabel);
        magazinePrefab = (GameObject)EditorGUILayout.ObjectField("Magazine", magazinePrefab, typeof(GameObject), false);
        scopePrefab = (GameObject)EditorGUILayout.ObjectField("Scope / Sight", scopePrefab, typeof(GameObject), false);
        attachmentPrefab = (GameObject)EditorGUILayout.ObjectField("Muzzle / Attachment", attachmentPrefab, typeof(GameObject), false);

        GUILayout.Space(20);

        EditorGUI.BeginDisabledGroup(bodyPrefab == null);
        if (GUILayout.Button("Assemble & Setup Weapon", GUILayout.Height(40)))
        {
            BuildWeapon();
        }
        EditorGUI.EndDisabledGroup();
        
        if (bodyPrefab == null)
        {
            EditorGUILayout.HelpBox("You must assign a Body Prefab to build the weapon.", MessageType.Warning);
        }
    }

    private void BuildWeapon()
    {
        // 1. Create Root Object
        GameObject weaponRoot = new GameObject(weaponName);
        Undo.RegisterCreatedObjectUndo(weaponRoot, "Build Modular Weapon");

        // Add Core Components
        weaponRoot.AddComponent<Animator>();
        Weapon weapon = weaponRoot.AddComponent<Weapon>();
        WeaponAttachmentManager attachmentManager = weaponRoot.AddComponent<WeaponAttachmentManager>();
        weaponRoot.AddComponent<AnimationReceiver>(); // Prevents animation event errors

        // 2. Create Socket Hierarchy
        Transform socketsRoot = CreateChild(weaponRoot.transform, "Sockets");
        Transform ejectionSocket = CreateChild(socketsRoot, "Socket_Ejection");
        Transform muzzleSocket = CreateChild(socketsRoot, "Socket_Muzzle");

        // Link Ejection Socket
        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("socketEjection").objectReferenceValue = ejectionSocket;
        weaponSO.ApplyModifiedProperties();

        // 3. Assemble Modular Parts
        // Synty modular pieces usually share a 0,0,0 origin, so parenting them to the root aligns them automatically.
        GameObject bodyObj = InstantiatePart(bodyPrefab, weaponRoot.transform, "Body");
        GameObject barrelObj = InstantiatePart(barrelPrefab, weaponRoot.transform, "Barrel");
        InstantiatePart(stockPrefab, weaponRoot.transform, "Stock");
        InstantiatePart(gripPrefab, weaponRoot.transform, "Grip");
        InstantiatePart(handguardPrefab, weaponRoot.transform, "Handguard");
        
        GameObject magObj = InstantiatePart(magazinePrefab, weaponRoot.transform, "Magazine");
        GameObject scopeObj = InstantiatePart(scopePrefab, weaponRoot.transform, "Scope");
        GameObject attachObj = InstantiatePart(attachmentPrefab, weaponRoot.transform, "Attachment");

        // 4. Assign LPSP Scripts to the correct modular parts
        
        // Scope Setup
        Scope scopeScript = null;
        if (scopeObj != null)
        {
            scopeScript = scopeObj.AddComponent<Scope>();
        }

        // Magazine Setup
        Magazine magScript = null;
        if (magObj != null)
        {
            magScript = magObj.AddComponent<Magazine>();
        }

        // Muzzle Setup (Attach to the attachment prefab, or the barrel if no attachment exists)
        Muzzle muzzleScript = null;
        GameObject muzzleTarget = attachObj != null ? attachObj : (barrelObj != null ? barrelObj : weaponRoot);
        muzzleScript = muzzleTarget.AddComponent<Muzzle>();

        SerializedObject muzzleSO = new SerializedObject(muzzleScript);
        muzzleSO.FindProperty("socket").objectReferenceValue = muzzleSocket;
        muzzleSO.ApplyModifiedProperties();

        // 5. Link everything to the Attachment Manager
        SerializedObject managerSO = new SerializedObject(attachmentManager);
        
        if (scopeScript != null) 
            managerSO.FindProperty("scopeDefaultBehaviour").objectReferenceValue = scopeScript;
        
        // Link Muzzle
        SerializedProperty muzzleArray = managerSO.FindProperty("muzzleArray");
        muzzleArray.arraySize = 1;
        muzzleArray.GetArrayElementAtIndex(0).objectReferenceValue = muzzleScript;

        // Link Magazine
        if (magScript != null)
        {
            SerializedProperty magazineArray = managerSO.FindProperty("magazineArray");
            magazineArray.arraySize = 1;
            magazineArray.GetArrayElementAtIndex(0).objectReferenceValue = magScript;
        }

        managerSO.ApplyModifiedProperties();

        // Select the newly built weapon in the hierarchy
        Selection.activeGameObject = weaponRoot;
        Debug.Log($"<color=cyan>Modular Weapon '{weaponName}' assembled successfully!</color>");
    }

    private GameObject InstantiatePart(GameObject prefab, Transform parent, string partName)
    {
        if (prefab == null) return null;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = $"Model_{partName}";
        instance.transform.SetParent(parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        return instance;
    }

    private Transform CreateChild(Transform parent, string childName)
    {
        Transform child = new GameObject(childName).transform;
        child.SetParent(parent);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        return child;
    }
}