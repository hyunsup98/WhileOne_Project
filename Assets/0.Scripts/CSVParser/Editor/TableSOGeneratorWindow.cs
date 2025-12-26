#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// CSV 파일의 데이터를 보관할 껍데기 Scriptable Object 정의 클래스를 만드는 에디터 창
/// </summary>
public class TableSOGeneratorWindow : EditorWindow
{
    #region SO 정의 클래스 생성에 필요한 변수
    // 각 경로의 폴더
    private DefaultAsset csvFolder;
    private DefaultAsset dataSoClassFolder;
    private DefaultAsset databaseSOClassFolder;

    // 각 필드에 들어간 값을 저장하는 키
    private const string CSVKEY = "CsvKey";
    private const string DATASOCLASSKEY = "DataSOClassKey";
    private const string DATABASESOClassKEY = "DatabaseSOClassKey";

    private static string csvFolderPath = "Assets/6.DataTable/CSV";                         // CSV 파일 폴더 경로
    private static string dataSOClassFolderPath = "Assets/6.DataTable/DataSO";              // 데이터 SO를 생성할 폴더 경로
    private static string databaseSOClassFolderPath = "Assets/6.DataTable/DatabaseSO";      // 데이터베이스 SO를 생성할 폴더 경로
    #endregion

    #region 실제 데이터가 들어간 SO 파일을 생성하기 위한 변수
    private DefaultAsset dataSOFolder;
    private DefaultAsset databaseSOFolder;

    private const string DATASOKEY = "DataSOKey";
    private const string DATABASESOKEY = "DatabaseSOKey";

    private static string dataSOFolderPath = "Assets/6.DataTable/SO/Data";                  // 데이터 SO가 저장될 경로
    private static string databaseSOFolderPath = "Assets/6.DataTable/SO/Database";          // 데이터베이스 SO가 저장될 경로
    #endregion

    #region 
    private TextAsset stringTableFolder;                                                    // StringTable.csv 파일
    private const string STRINGTABLEKEY = "StringTableKey";                                 // StringTable.csv 데이터 저장 키
    private static string stringTableFolderPath = "Assets/6.DataTable/StringTable";         // StringTable 저장 경로
    #endregion

    [MenuItem("Cooperation Tool/Data/Table Scriptable Object Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TableSOGeneratorWindow));
    }

    /// <summary>
    /// MonoBehariour의 Update와 같은 역할이라고 생각하면 됨
    /// OnGui 함수 안에 작성한 내용은 매 프레임 단위로 나옴
    /// </summary>
    private void OnGUI()
    {
        // 소제목 Label 스타일 설정
        // 1. 폰트스타일 → bold
        // 2. 자동 줄바꿈 → true
        GUIStyle subTitleLabelStyle = new GUIStyle(EditorStyles.label);
        subTitleLabelStyle.fontStyle = FontStyle.Bold;
        subTitleLabelStyle.wordWrap = true;

        // 본문 Label 스타일 설정
        // 1. 자동 줄바꿈 → true
        GUIStyle bodyLabelStyle = new GUIStyle(EditorStyles.label);
        bodyLabelStyle.wordWrap = true;

        // 줄바꿈, 매개변수로 float 값을 줄 수도 있음
        EditorGUILayout.Space();

        #region StringTable.csv 파일 따로 받아오기
        // --- 1. 에디터 창에 대한 간략한 설명 ---
        //생성된 창 내용에 라벨(소제목)을 넣는 함수, 3가지의 매개변수를 가짐 → 텍스트, 텍스트 스타일, 옵션
        GUILayout.Label("1. StringTable 설정", subTitleLabelStyle, GUILayout.Height(20));
        GUILayout.Label("StringTable.csv 파일을 받아와 추후 생성할 데이터들이 참조하게 합니다.", bodyLabelStyle, GUILayout.Height(30));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        DrawTextAssetField("StringTable.csv를 넣어주세요", ref stringTableFolder, ref stringTableFolderPath, STRINGTABLEKEY);
        #endregion

        EditorGUILayout.Space();

        #region CSV 파일 → SO 정의 클래스로 변환하기 위한 창

        // --- 2. 에디터 창에 대한 간략한 설명 ---
        //생성된 창 내용에 라벨(소제목)을 넣는 함수, 3가지의 매개변수를 가짐 → 텍스트, 텍스트 스타일, 옵션
        GUILayout.Label("2. CSV 파일 경로 및 SO 정의 클래스 경로 설정", subTitleLabelStyle, GUILayout.Height(20));
        GUILayout.Label("지정된 경로에 있는 CSV 파일 데이터를 SO 형식 데이터 컨테이너로 생성합니다.", bodyLabelStyle, GUILayout.Height(30));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        DrawFolderField("CSV 폴더 경로를 지정해주세요", ref csvFolder, ref csvFolderPath, CSVKEY);

        EditorGUILayout.Space();
        DrawFolderField("Data SO 파일을 생성할 폴더 경로를 지정해주세요", ref dataSoClassFolder, ref dataSOClassFolderPath, DATASOCLASSKEY);

        EditorGUILayout.Space();
        DrawFolderField("Database SO 파일을 생성할 폴더 경로를 지정해주세요", ref databaseSOClassFolder, ref databaseSOClassFolderPath, DATABASESOClassKEY);

        #endregion

        EditorGUILayout.Space(20);

        #region 실제 CSV 파일 데이터를 SO 파일로 생성

        // --- 3. 에디터 창에 대한 간략한 설명 ---
        GUILayout.Label("3. SO 파일 생성 경로 설정", subTitleLabelStyle, GUILayout.Height(20));
        GUILayout.Label("CSV 파일의 실제 데이터와 위에서 생성한 SO 정의 클래스를 이용해" +
            "\n실제 데이터가 들어간 SO 데이터를 생성합니다.", bodyLabelStyle, GUILayout.Height(60));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        DrawFolderField("데이터를 저장할 SO 저장 경로를 지정해주세요", ref dataSOFolder, ref dataSOFolderPath, DATASOKEY);

        EditorGUILayout.Space();
        DrawFolderField("데이터 SO들을 저장할 SO 저장 경로를 지정해주세요", ref databaseSOFolder, ref databaseSOFolderPath, DATABASESOKEY);

        // --- 4. 클래스 생성 버튼 ---
        EditorGUILayout.Space(20);


        // 이 코드 이후 그려지는 GUI 컨트롤의 상호 작용 여부를 켜거나 끔
        GUI.enabled = IsValid(stringTableFolderPath) &&
            IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath) &&
            IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath, dataSOFolderPath, databaseSOFolderPath);

        if (GUILayout.Button("CSV 파싱 시작", GUILayout.Height(30)))
        {
            // StringTable.csv 파일 자료구조에 넣는 메서드
            TableSOGenerator.ParseStringTable(stringTableFolder);
            // SO 정의 클래스 생성 메서드
            TableSOGenerator.GenerateAllClassedInFolder(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath);
            // SO 파일 생성 메서드
            // OnClick_CreateSO(); 메서드는 [DidReloadScripts] 어트리뷰트가 있어서 호출해주지 않아도 에디터 리로드 시 자동 호출
        }

        GUI.enabled = true;

        if (!IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath, dataSOFolderPath, databaseSOFolderPath))
        {
            EditorGUILayout.HelpBox("모든 파일 및 폴더 경로를 지정해주세요.", MessageType.Error);
        }
        #endregion

        // 이 코드 이후 그려지는 GUI 컨트롤의 상호 작용 여부를 켜거나 끔
        GUI.enabled = IsValid(stringTableFolderPath) &&
            IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath) &&
            IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath, dataSOFolderPath, databaseSOFolderPath);

        if (GUILayout.Button("SO 생성 시작", GUILayout.Height(30)))
        {
            // SO 파일 생성 메서드
            OnClick_CreateSO();
        }

        GUI.enabled = true;

        if (!IsValid(csvFolderPath, dataSOClassFolderPath, databaseSOClassFolderPath, dataSOFolderPath, databaseSOFolderPath))
        {
            EditorGUILayout.HelpBox("모든 파일 및 폴더 경로를 지정해주세요.", MessageType.Error);
        }
    }

    // DidReloadScripts → 스크립트가 로드된 후 특정 메소드를 호출
    // 하지만 의도한 스크립트가 생기는 것 외에도 에디터가 변해서 저장되고 로드되면 무조건 호출되기 때문에 의도된 사항은 아님, 추후 고쳐야 됨
    // 직접 버튼 클릭 등의 이벤트로 메서드를 부르지 않아도 에디터가 리로드될 때 무조건 실행됨
    //[DidReloadScripts]
    private static void OnClick_CreateSO()
    {
        TableSOGenerator.CreateSOFileFromScript(csvFolderPath, dataSOClassFolderPath, dataSOFolderPath, databaseSOClassFolderPath, databaseSOFolderPath);
    }

    /// <summary>
    /// 폴더를 드래그할 오브젝트 필드를 만들고 에디터 내부에 저장하는 메서드
    /// </summary>
    /// <param name="label"> 라벨 </param>
    /// <param name="folderAsset"> 선택된 폴더 </param>
    /// <param name="path"> 폴더 경로 </param>
    /// <param name="key"> EditorPrefs에 저장할 키 </param>
    private void DrawFolderField(string label, ref DefaultAsset folderAsset, ref string path, string key)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        folderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
            path,
            folderAsset,
            typeof(DefaultAsset),
            false
            );

        if (folderAsset == null)
        {
            path = null;
            EditorPrefs.DeleteKey(key);
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(folderAsset);

        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            EditorGUILayout.HelpBox("폴더만 지정할 수 있습니다.", MessageType.Error);
            EditorPrefs.DeleteKey(key);
            return;
        }

        path = assetPath;
        EditorPrefs.SetString(key, assetPath);
    }

    private void DrawTextAssetField(string label, ref TextAsset textAsset, ref string path, string key)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        textAsset = (TextAsset)EditorGUILayout.ObjectField(
            path,
            textAsset,
            typeof(TextAsset),
            false
            );

        if (textAsset == null)
        {
            path = null;
            EditorPrefs.DeleteKey(key);
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(textAsset);

        if (!assetPath.EndsWith(".csv") && !assetPath.EndsWith(".txt"))
        {
            EditorGUILayout.HelpBox("CSV 또는 TXT 파일 형식만 지정할 수 있습니다.", MessageType.Error);
            EditorPrefs.DeleteKey(key);
            return;
        }

        path = assetPath;
        EditorPrefs.SetString(key, assetPath);
    }

    /// <summary>
    /// 모든 경로에 값이 들어있는지 확인
    /// </summary>
    /// <returns> 모든 경로에 값이 들어있으면 true, 아니면 false </returns>
    private bool IsValid(params string[] path)
    {
        bool isValid = true;

        foreach(var p in path)
        {
            if(string.IsNullOrEmpty(p))
                isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// 폴더 변수에 경로에 있는 폴더 연결
    /// </summary>
    /// <param name="asset"> 값을 넣어줄 폴더 변수 </param>
    /// <param name="path"> 폴더 경로 </param>
    private void RestoreFolderAsset(ref DefaultAsset asset, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
        }
    }
    private void RestoreTextAsset(ref TextAsset asset, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
    }

    private void OnEnable()
    {
        //에디터 창이 켜질 때 최소 사이즈 설정
        minSize = new Vector2(300, 500);

        csvFolderPath = EditorPrefs.GetString(CSVKEY);
        dataSOClassFolderPath = EditorPrefs.GetString(DATASOCLASSKEY);
        databaseSOClassFolderPath = EditorPrefs.GetString(DATABASESOClassKEY);
        dataSOFolderPath = EditorPrefs.GetString(DATASOKEY);
        databaseSOFolderPath = EditorPrefs.GetString(DATABASESOKEY);
        stringTableFolderPath = EditorPrefs.GetString(STRINGTABLEKEY);

        RestoreFolderAsset(ref csvFolder, csvFolderPath);
        RestoreFolderAsset(ref dataSoClassFolder, dataSOClassFolderPath);
        RestoreFolderAsset(ref databaseSOClassFolder, databaseSOClassFolderPath);
        RestoreFolderAsset(ref dataSOFolder, dataSOFolderPath);
        RestoreFolderAsset(ref databaseSOFolder, databaseSOFolderPath);
        RestoreTextAsset(ref stringTableFolder, stringTableFolderPath);
    }
}
#endif