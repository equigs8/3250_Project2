using UnityEngine;
using UnityEditor;
using InfimaGames.LowPolyShooterPack;

public class SyntyWeaponSetupTool : EditorWindow
{
    private GameObject targetWeapon;

    [MenuItem("Tools/Synty to LPSP Weapon Setup")]
    public static void ShowWindow()
    {
        // Opens the custom editor window
        GetWindow<SyntyWeaponSetupTool>("Weapon Setup Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Synty Polygon Weapon Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Select a Synty weapon model from your scene, drag it below, and click 'Setup Weapon'. This will automatically generate the Infima LPSP hierarchy and components.", MessageType.Info);

        targetWeapon = (GameObject)EditorGUILayout.ObjectField("Target Weapon", targetWeapon, typeof(GameObject), true);

        GUILayout.Space(20);

        if (GUILayout.Button("Setup Weapon", GUILayout.Height(40)))
        {
            if (targetWeapon != null)
            {
                SetupWeapon(targetWeapon);
            }
            else
            {
                Debug.LogWarning("Please assign a target weapon first!");
            }
        }
    }

    private void SetupWeapon(GameObject weaponRoot)
    {
        // 1. Add Core Components
        if (!weaponRoot.GetComponent<Animator>()) weaponRoot.AddComponent<Animator>();
        
        Weapon weapon = weaponRoot.GetComponent<Weapon>();
        if (weapon == null) weapon = weaponRoot.AddComponent<Weapon>();

        WeaponAttachmentManager attachmentManager = weaponRoot.GetComponent<WeaponAttachmentManager>();
        if (attachmentManager == null) attachmentManager = weaponRoot.AddComponent<WeaponAttachmentManager>();

        // 2. Create Socket Hierarchy
        Transform socketsRoot = CreateChild(weaponRoot.transform, "Sockets");
        Transform ejectionSocket = CreateChild(socketsRoot, "Socket_Ejection");
        Transform muzzleSocket = CreateChild(socketsRoot, "Socket_Muzzle");
        Transform magazineSocket = CreateChild(socketsRoot, "Socket_Magazine");
        Transform scopeSocket = CreateChild(socketsRoot, "Socket_Scope");

        // 3. Link the Ejection Socket to the Weapon script using SerializedObject 
        // (because socketEjection is a private serialized field)
        SerializedObject weaponSO = new SerializedObject(weapon);
        weaponSO.FindProperty("socketEjection").objectReferenceValue = ejectionSocket;
        weaponSO.ApplyModifiedProperties();

        // 4. Setup Default Attachments
        
        // Setup Muzzle
        Muzzle muzzle = muzzleSocket.gameObject.GetComponent<Muzzle>();
        if (muzzle == null) muzzle = muzzleSocket.gameObject.AddComponent<Muzzle>();
        SerializedObject muzzleSO = new SerializedObject(muzzle);
        muzzleSO.FindProperty("socket").objectReferenceValue = muzzleSocket; // Use itself as the firing point
        muzzleSO.ApplyModifiedProperties();

        // Setup Magazine
        Magazine magazine = magazineSocket.gameObject.GetComponent<Magazine>();
        if (magazine == null) magazine = magazineSocket.gameObject.AddComponent<Magazine>();

        // Setup Scope
        Scope scope = scopeSocket.gameObject.GetComponent<Scope>();
        if (scope == null) scope = scopeSocket.gameObject.AddComponent<Scope>();

        // 5. Link Attachments to the Attachment Manager
        SerializedObject managerSO = new SerializedObject(attachmentManager);
        managerSO.FindProperty("scopeDefaultBehaviour").objectReferenceValue = scope;
        
        // Link Muzzle Array
        SerializedProperty muzzleArray = managerSO.FindProperty("muzzleArray");
        muzzleArray.arraySize = 1;
        muzzleArray.GetArrayElementAtIndex(0).objectReferenceValue = muzzle;

        // Link Magazine Array
        SerializedProperty magazineArray = managerSO.FindProperty("magazineArray");
        magazineArray.arraySize = 1;
        magazineArray.GetArrayElementAtIndex(0).objectReferenceValue = magazine;

        managerSO.ApplyModifiedProperties();

        // 6. Optional: Add an AnimationReceiver to prevent Unity errors if playing animations in isolation
        if (!weaponRoot.GetComponent<AnimationReceiver>()) weaponRoot.AddComponent<AnimationReceiver>();

        Debug.Log($"<color=green>Successfully setup {weaponRoot.name}!</color> Please adjust socket positions and assign your prefabs (Projectiles, Casings, UI Sprites).");
    }

    private Transform CreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            child = new GameObject(childName).transform;
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }
        return child;
    }
}