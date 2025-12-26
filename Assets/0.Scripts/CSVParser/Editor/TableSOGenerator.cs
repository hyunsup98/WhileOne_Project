#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TableSOGenerator
{
    // CSV 헤더 행 인덱스 설정
    private const int ColumnDescRow = 0;                // 어떤 컬럼인지에 대한 설명
    private const int ColumnInternalNameRow = 1;        // 컬럼명 (내부 변수 이름)
    private const int DataTypeRow = 2;                  // 자료형 (int, float, bool, string) 타입
    private const int DataRow = 3;                      // 실제 데이터가 시작되는 열

    // 클래스를 작성할 때 사용할 stringbuilder
    private static StringBuilder sb_fileName = new StringBuilder();

    // string 테이블은 값을 따로 저장
    private static Dictionary<string, string> stringTables = new Dictionary<string, string>();

    #region SO 정의 클래스 셍성 메서드
    /// <summary>
    /// 저장된 폴더 내의 모든 TextAsset(CSV) 파일을 읽어 클래스 코드를 생성하고 저장합니다.
    /// </summary>
    /// <param name="dataTableFolderPath"> CSV 파일이 위치한 Assets 내 폴더 경로 </param>
    /// <param name="dataSOPath"> 생성한 클래스 팡리을 저장할 Assets 내 폴더 경로 </param>
    /// <param name="databaseSOPath"> 생성한 클래스가 상속할 베이스 클래스 이름 </param>
    public static void GenerateAllClassedInFolder(string dataTableFolderPath, string dataSOPath, string databaseSOPath)
    {
        // 1. 폴더 내 모든 TextAsset 파일의 Asset Path 목록을 가져옵니다.
        // AssetDatabase.FindAssets → 프로젝트 내 에셋을 검색해서 GUID 배열로 반환
        // assetGUIDs에는 찾은 TextAsset 파일들의 GUID 값이 들어감 (예: "e2a3f7c1b8d24a34a8b6c4c9e1234567")
        // "t:TextAssset" → TextAsset 타입인 에셋만 찾아라(.txt | .csv | .json | .bytes)
        // new[] {dataTableFolderPath} → dataTableFolderPath 경로 하위의 폴더로 검색 범위 제한
        string[] assetGUIDs = AssetDatabase.FindAssets($"t:TextAsset", new[] { dataTableFolderPath });

        // 해당 경로에서 찾은 TextAsset 타입의 파일이 없을 경우
        if (assetGUIDs.Length == 0)
        {
            Debug.LogWarning($"[Generator WARNING] 경로 '{dataTableFolderPath}' 에서 TextAsset 파일을 찾을 수 없습니다.");
            EditorUtility.DisplayDialog("클래스 생성 완료", $"경로 '{dataTableFolderPath}' 에서 CSV 파일을 찾지 못했습니다.", "확인");

            return;
        }

        // 총 몇개의 클래스 파일이 생성되었는지 세는 변수
        int successCount = 0;

        // 2. 각 TextAsset 파일에 대해 파싱 및 생성 반복
        foreach (string guid in assetGUIDs)
        {
            // GUID를 해당 에셋 경로로 반환
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 지정된 경로에 있는 에셋을 TextAsset 타입으로 로드(반환)
            TextAsset csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

            if (csvFile == null) continue;

            Debug.Log($"[Generator] CSV 파일 처리 시작: {csvFile.name}");

            CsvClassData data = ParseCsvHeaders(csvFile);
            if (data == null) continue;

            sb_fileName.Clear();
            sb_fileName.Append($"{data.TableName.Replace("Table", "").Trim()}DataSO");

            // SO 데이터 생성
            string code = GenerateDataSO(data);
            SaveClassFile(code, sb_fileName.ToString(), dataSOPath, false);

            sb_fileName.Clear();
            sb_fileName.Append($"{data.TableName.Replace("Table", "").Trim()}DatabaseSO");

            code = GenerateDatabaseSO(data);
            SaveClassFile(code, sb_fileName.ToString(), databaseSOPath, false);
            successCount++;
        }

        // 3. 최종 결과 알림
        AssetDatabase.Refresh();    // 최종적으로 한 번만 새로고침
        Debug.Log($"[Generator SUCCESS] 총 {successCount * 2}개의 클래스 파일 생성 완료.");
        EditorUtility.DisplayDialog("클래스 생성 완료", $"총 {successCount}개의 SO 데이터 클래스 파일이 다음 경로에 생성되었습니다:\n{dataSOPath}" +
            $"\n총 {successCount}개의 SO 데이터베이스 클래스 파일이 다음 경로에 생성되었습니다:\n{databaseSOPath}", "확인");
    }

    /// <summary>
    /// CSV 파일을 읽어 클래스 생성에 필요한 데이터 구조를 반환합니다.
    /// </summary>
    /// <param name="csvFile"> CSV 파일 </param>
    /// <returns> 작성한 CsvClassData 파일 </returns>
    public static CsvClassData ParseCsvHeaders(TextAsset csvFile)
    {
        if (csvFile == null) return null;

        CsvClassData classData = new CsvClassData
        {
            // Path.GetFileNameWithoutExtension() → 확장자를 제외한 파일 이름을 반환
            // GetFileNameWithoutExtension() 메서드의 매개변수가 string path인데 왜 파일 이름을 넣어도 되는걸까?
            // → 어차피 경로를 넣어도 내부적으로 마지막의 파일명만 추출하기 때문에 파일명만 줘도 됨
            // csvFile.name을 바로 줘도 괜찮겠지만 확실하고 안전한 방법을 위해 메서드 사용
            TableName = Path.GetFileNameWithoutExtension(csvFile.name)
        };

        // 1. CSV 내용을 행(줄) 단위로 분리
        // '\r'와 '\n'를 기준으로 분리
        // System.StringSplitOptions.RemoveEmptyEntries → 분리 결과중 빈 문자열은 결과 배열에 포함하지 않는다.
        // string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);        // 기존의 방식, 셀 내의 줄바꿈도 잘라버려서 이상하게 분리됨
        string[] lines = Regex.Split(csvFile.text, @"\r\n|\n(?=(?:[^""]*""[^""]*"")*[^""]*$)");                     // 큰따옴표 내의 줄바꿈은 무시

        //데이터 타입이 적힌 행보다 lines의 길이가 
        if (lines.Length <= DataTypeRow)
        {
            Debug.LogError($"[Generator ERROR] CSV 파일 '{csvFile.name}'의 헤더 행이 부족합니다.");
            return null;
        }

        // 2. 컬럼의 설명과 컬럼명(내부명)과 자료형 행 추출
        string[] dataDesc = Regex.Split(lines[ColumnDescRow], @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");
        string[] internalNames = Regex.Split(lines[ColumnInternalNameRow], @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");
        string[] dataTypes = Regex.Split(lines[DataTypeRow], @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        int columnCount = dataDesc.Length;
        if (columnCount != internalNames.Length || columnCount != dataTypes.Length)
        {
            Debug.LogError($"[Generator ERROR] 컬럼설명({columnCount})과 컬럼명({internalNames.Length}), 자료형({dataTypes.Length})의 수가 일치하지 않습니다.");
            return null;
        }

        // 3. 컬럼 정의 리스트 생성
        for (int i = 0; i < columnCount; i++)
        {
            string desc = dataDesc[i];
            string name = internalNames[i];
            string type = dataTypes[i];

            if (string.IsNullOrEmpty(desc) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
            {
                // 이름이나 타입이 비어있는 컬럼은 무시
                continue;
            }

            classData.Columns.Add(new ColumnDefinition { Name = name, Type = ConvertCSharpType(type), Desc = desc });
        }

        if(lines.Length > DataRow)
        {
            // 현재 테이블에 총 몇 개의 데이터가 있는지 저장
            classData.DataCount = lines.Length - DataRow;

            if(classData.DataCount > 0)
            {
                foreach(var col in classData.Columns)
                {
                    col.values = new string[classData.DataCount];
                }

                for (int i = 0; i < classData.DataCount; i++)
                {
                    string[] values = Regex.Split(lines[i + DataRow], @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");

                    for(int j = 0; j < values.Length; j++)
                    {
                        // 해당 컬럼 필드명에 "_Name" 또는 "_Desc"가 들어갈 경우 stringTable에서 가져와야 하는 값이라고 판단
                        // stringTables 딕셔너리에 접근하여 값 가져오기
                        if (classData.Columns[j].Name.Contains("_Name") || classData.Columns[j].Name.Contains("_Desc"))
                        {
                            if (stringTables.Keys.Contains(values[j]) && stringTables.TryGetValue(values[j], out var str))
                            {
                                Debug.Log(str);
                                classData.Columns[j].values[i] = str;
                                continue;
                            }
                        }

                        classData.Columns[j].values[i] = values[j];
                    }
                }
            }
        }

        return classData;
    }

    /// <summary>
    /// CSV 자료형 문자열을 C# 타입 문자열로 변환합니다.
    /// </summary>
    /// <param name="csvType"></param>
    /// <returns></returns>
    private static string ConvertCSharpType(string csvType)
    {
        // 소문자로 변환하여 비교
        switch (csvType.ToLower())
        {
            case "int":
            case "enum":
                return "int";
            case "float":
                return "float";
            case "bool":
                return "bool";
            case "string":
            default:
                return "string";
        }
    }

    /// <summary>
    /// Csv 파일을 기반으로 생성할 Scriptable Object를 정의하는 클래스를 만듭니다.
    /// </summary>
    /// <param name="data"> CSV 파일의 헤더 정보를 저장한 클래스 컨테이너 </param>
    /// <param name="dataSOPath"> SO 데이터를 저장할 경로 </param>
    /// <returns> SO 생성 정의 클래스 문자열 </returns>
    private static string GenerateDataSO(CsvClassData data)
    {
        string dataName = data.TableName.Replace("Table", "").Trim();
        string dataSOName = $"{dataName}DataSO";

        var idColumn = data.Columns[0];
        string idType = idColumn.Type;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{dataSOName}\", menuName = \"Scriptable Objects/Data/{dataSOName}\")]");
        sb.AppendLine($"public class {dataSOName} : TableBase<{idType}>");
        sb.AppendLine("{");

        foreach(var col in data.Columns)
        {
            // CSV 파일에 있는 필드 명에 따라 타입을 지정
            string type = col.Type;

            if(col.Name.ToLower().Contains("path_"))
            {
                // 필드명에 "Path_"가 들어가면

                if (col.Name.ToLower().Contains("_sprite"))
                {
                    // 필드명에 "_Sprite"가 들어가면 타입을 Sprite로 지정
                    type = "Sprite";
                }
                else if(col.Name.ToLower().Contains("_audioclip"))
                {
                    // 필드명에 "_AudioClip"이 들어가면 타입을 AudioClip으로 지정
                    type = "AudioClip";
                }
            }

            //프로퍼티 생성
            sb.AppendLine($"    // {col.Desc.Replace('\n', ' ')}");
            sb.AppendLine($"    [field: SerializeField] public {type} {col.Name} {{ get; private set; }}");
            sb.AppendLine();
        }

        sb.AppendLine("    // 부모 클래스의 ID 반환 추상 메서드");
        sb.AppendLine($"    public override {idType} GetID() => {idColumn.Name};");

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Csv 파일을 기반으로 생성할 데이터 SO를 보관할 총괄 SO를 정의하는 클래스를 만듭니다.
    /// </summary>
    /// <param name="data"> CSV 파일 데이터 </param>
    /// <param name="databaseSOPath"> 저장할 경로 </param>
    /// <returns> SO 생성 정의 클래스 문자열 </returns>
    private static string GenerateDatabaseSO(CsvClassData data)
    {
        string databaseName = data.TableName.Replace("Table", "").Trim();
        string databaseSOName = $"{databaseName}DatabaseSO";
        string dataSOName = $"{databaseName}DataSO";

        string idType = data.Columns[0].Type;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"[CreateAssetMenu(fileName = \"{databaseSOName}\", menuName = \"Scriptable Objects/DataBase/{databaseSOName}\")]");
        sb.AppendLine($"public class {databaseSOName} : TableDatabase<{idType}, {dataSOName}>");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine("}");

        return sb.ToString();
    }
    #endregion

    #region SO 파일 생성
    /// <summary>
    /// SO 정의 클래스를 이용해 실제 SO 파일을 생성
    /// </summary>
    /// <param name="csvPath"> CSV 파일 폴더 경로 </param>
    /// <param name="dataSOPath"> SO 정의 클래스가 있는 폴더 경로 </param>
    /// <param name="dataSOSavePath"> SO 정의 클래스를 저장할 경로 </param>
    /// <param name="databaseSOPath"> SO 파일 데이터베이스 정의 클래스가 있는 폴더 경로 </param>
    /// <param name="databaseSOSavePath"> SO 데이터베이스 정의 클래스를 저장할 경로 </param>
    public static void CreateSOFileFromScript(string csvPath, string dataSOPath, string dataSOSavePath, string databaseSOPath, string databaseSOSavePath)
    {
        // dataSOPath에 위치한 모든 스크립트를 받아옴 → guid 형식
        string[] dataSOGUIDs = AssetDatabase.FindAssets($"t:MonoScript", new[] { dataSOPath });

        // 지정된 경로에 스크립트 파일이 존재하지 않을 경우
        if (dataSOGUIDs.Length == 0)
        {
            Debug.Log("지정된 경로에 스크립트 파일이 존재하지 않습니다.");
            return;
        }

        // 해당 폴더에서 찾은 SO 정의 클래스만큼 돌려봄
        for(int i = 0; i < dataSOGUIDs.Length; i++)
        {
            string scriptPath = AssetDatabase.GUIDToAssetPath(dataSOGUIDs[i]);
            MonoScript scriptInfo = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            Type soType = scriptInfo.GetClass();

            #region 데이터베이스 SO 생성
            // 이번 데이터 SO와 매칭되는 데이터베이스 SO 클래스 파일 경로 찾기
            // 데이터 SO는 ~DataSO / 데이터베이스 SO는 ~DatabaseSO 이기에 이름으로 찾음
            string dbSOPath = Path.Combine(databaseSOPath, $"{soType.ToString().Replace("SO", "baseSO")}.cs");

            // 데이터 SO들을 담을 리스트 생성
            System.Collections.IList dbList = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(soType));
            #endregion

            string folderPath = Path.Combine(dataSOSavePath, $"{soType.ToString()}/");

            // filePath 경로가 없다면 폴더 생성해줌
            if(!Directory.Exists(Path.GetDirectoryName(folderPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(folderPath));
            }

            if (soType == null)
            {
                Debug.Log("유효한 클래스 타입이 아닙니다.");
                continue;
            }

            // CSV 파일 불러오기
            string csvFileName = string.Format("{0}Table", scriptInfo.GetClass().ToString().Replace("DataSO", "").Trim());
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:TextAsset", new[] { csvPath });

            CsvClassData csvClassData = new CsvClassData();

            // 해당 경로에서 찾은 TextAsset 타입의 파일이 없을 경우
            if (assetGUIDs.Length == 0)
            {
                Debug.LogWarning($"[Generator WARNING] 경로 '{csvPath}' 에서 TextAsset 파일을 찾을 수 없습니다.");
                EditorUtility.DisplayDialog("클래스 생성 완료", $"경로 '{csvPath}' 에서 CSV 파일을 찾지 못했습니다.", "확인");

                continue;
            }

            // csvFileName에 맞는 CSV 파일 찾기
            foreach(var guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if(Path.GetFileNameWithoutExtension(path) == csvFileName && path.EndsWith(".csv"))
                {
                    csvClassData = ParseCsvHeaders(AssetDatabase.LoadAssetAtPath<TextAsset>(path));
                    continue;
                }

                Debug.LogWarning($"{csvFileName}에 맞는 TextAsset 파일을 찾을 수 없습니다.");
                continue;
            }

            // SO 파일 생성 및 값 넣어주기
            for(int j = 0; j < csvClassData.DataCount; j++)
            {
                string filePath = Path.Combine(folderPath, $"{csvClassData.Columns[0].values[j]}.asset");

                ScriptableObject instance = ScriptableObject.CreateInstance(soType);
                InjectSOData(instance, csvClassData, j);

                AssetDatabase.CreateAsset(instance, filePath);

                dbList.Add(instance);
            }

            CreateDBSOFile(dbSOPath, databaseSOSavePath, dbList);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// SO를 모두 가지고있는 데이터베이스 SO 생성 메서드
    /// </summary>
    /// <param name="databaseSOPath"> 데이터베이스 SO 정의 클래스 파일 경로 </param>
    /// <param name="databaseSOSavePath"> 생성한 데이터베이스 SO를 저장할 폴더 경로 </param>
    /// <param name="soList"> 데이터 SO가 들어있는 리스트 </param>
    public static void CreateDBSOFile(string databaseSOPath, string databaseSOSavePath, System.Collections.IList soList)
    {
        // 데이터베이스 SO 파일이 있는 경로
        MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(databaseSOPath);

        if (mono == null) return;

        Type soType = mono.GetClass();

        // SO 생성
        ScriptableObject instance = ScriptableObject.CreateInstance(soType);

        // 리플렉션으로 리스트 가져오기
        FieldInfo listField = soType.GetField("datas", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        if (listField != null)
        {
            listField.SetValue(instance, soList);
        }

        // filePath 경로가 없다면 폴더 생성해줌
        if (!Directory.Exists(Path.GetDirectoryName(databaseSOSavePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(databaseSOSavePath));
        }

        string path = Path.Combine(databaseSOSavePath, $"{soType.ToString()}.asset");
        AssetDatabase.CreateAsset(instance, path);
    }

    /// <summary>
    /// 생성된 Scriptable Object에 데이터를 넣어주는 메서드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="soInstance"> 데이터가 들어갈 Scriptable Object 객체 </param>
    /// <param name="csvFile"> 데이터가 들어있는 CSV 파일 </param>
    /// <param name="index"> CSV 파일의 몇 번째 데이터값을 넣을 것인지 </param>
    public static void InjectSOData<T>(T soInstance, CsvClassData csvFile, int index)
    {
        Type soType = soInstance.GetType();

        for(int i = 0; i < csvFile.Columns.Count; i++)
        {
            var column = csvFile.Columns[i];
            string value = csvFile.Columns[i].values[index];

            // 컬럼 이름과 똑같은 이름의 프로퍼티를 찾기
            PropertyInfo prop = soType.GetProperty(csvFile.Columns[i].Name);
            // 타입을 넣을 오브젝트

            // prop이 null이 아니고 쓰기 가능할 때
            if(prop != null && prop.CanWrite)
            {
                if (string.IsNullOrEmpty(value)) 
                    continue;

                try
                {
                    // 타입을 넣을 오브젝트
                    object type = null;
                    // 필드명 이름 → 비교를 위해 소문자로 변환
                    string columnName = csvFile.Columns[i].Name.ToLower();

                    if (columnName.Contains("path_"))
                    {
                        // value(주소)를 바탕으로 파일명만 추출
                        string fileName = Path.GetFileName(value);
                        // value(주소)를 바탕으로 폴더 경로만 추출
                        string directory = Path.GetDirectoryName(value);

                        // 필드명에서 타입을 추출하기 위해 처음부터 '_'까지 다 자름
                        // 예: MonsterImgResourcePath_Sprite → Sprite
                        string strType = columnName.Substring(columnName.IndexOf('_') + 1);
                        // FindAssets 메서드를 쓸 때 파일 타입을 지정해주기 위함
                        // 예: t:Sprite FileName → FileName의 스프라이트 타입을 다 찾아줌
                        string path = $"t:{strType} {fileName}";

                        // directory 변수에 들어있는 경로에서 strType의 path 이름 파일을 모두 찾음 (guid 형태로 반환)
                        string[] guids = AssetDatabase.FindAssets(path, new[] { directory });

                        if(guids.Length > 0)
                        {
                            // guid 형태로 받은 파일의 경로를 반환
                            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                            // type에 해당 경로의 파일 넣어주기
                            type = AssetDatabase.LoadAssetAtPath(assetPath, prop.PropertyType);
                        }
                        else
                        {
                            Debug.LogWarning($"{directory} 경로의 {fileName} 파일을 찾을 수 없습니다.");
                        }
                    }
                    else
                    {
                        // 필드명에 "Path_"가 안들어가면 아래 실행
                        type = Convert.ChangeType(value, prop.PropertyType);
                    }

                    prop.SetValue(soInstance, type);
                }
                catch(Exception e)
                {
                    Debug.LogError($"{column.Name} 필드 데이터 변환 실패: {e.Message}");
                }
            }
        }
    }
    #endregion

    #region StringTable.csv의 내용을 stringTables 딕셔너리에 넣기
    /// <summary>
    /// StringTable.csv 파일을 받와와서 stringTables 딕셔너리 자료 구조에 넣기
    /// 추후 다른 데이터 테이블에서 키로 접근 후 해당 키의 값을 저장하기 위함
    /// </summary>
    /// <param name="stringTable"> StringTable.csv 파일 </param>
    public static void ParseStringTable(TextAsset stringTable)
    {
        string[] lines = Regex.Split(stringTable.text, @"\r\n|\n(?=(?:[^""]*""[^""]*"")*[^""]*$)");                     // 큰따옴표 내의 줄바꿈은 무시

        //데이터 타입이 적힌 행보다 lines의 길이가 
        if (lines.Length <= DataTypeRow)
        {
            Debug.LogError($"[Generator ERROR] CSV 파일 '{stringTable.name}'의 헤더 행이 부족합니다.");
            return;
        }

        int dataCount = lines.Length - DataRow;
        for (int i = 0; i < dataCount; i++)
        {
            string[] values = Regex.Split(lines[i + DataRow], @",(?=(?:[^""]*""[^""]*"")*[^""]*$)");
            stringTables[values[0]] = values[1];
        }
    }
    #endregion

    /// <summary>
    /// 생성된 C# 코드를 파일로 저장합니다.
    /// </summary>
    /// <param name="code"> 저장할 C# 클래스 코드 </param>
    /// <param name="className"> 생성될 클래스 이름 </param>
    /// <param name="savePath"> 저장할 폴더 경로 </param>
    /// <param name="dialogEnabled"> 저장 성공/실패 대화 상자를 표시할지 여부 </param>
    private static void SaveClassFile(string code, string className, string savePath, bool dialogEnabled = true)
    {
        // 파일 경로 설정
        string fullPath = Path.Combine(savePath, $"{className}.cs");

        try
        {
            // 폴더가 없으면 생성
            Directory.CreateDirectory(savePath);
            File.WriteAllText(fullPath, code, Encoding.UTF8);

            if (dialogEnabled)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[Generator SUCCESS] 클래스 파일 생성 완료: {fullPath}");
                EditorUtility.DisplayDialog("클래스 생성 성공", $"'{className}.cs' 파일이 다음 경로에 생성되었습니다:\n{savePath}", "확인");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Generator ERROR] 파일 저장 실패: {e.Message}");
            if (dialogEnabled)
            {
                EditorUtility.DisplayDialog("클래스 생성 실패", $"파일 저장 중 오류가 발생했습니다:\n{e.Message}", "확인");
            }
        }
    }
}
#endif