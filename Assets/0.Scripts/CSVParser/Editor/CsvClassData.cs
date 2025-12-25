using System.Collections.Generic;

/// <summary>
/// CSV 파일의 컬럼 정의(이름과 타입, 주석에 들어갈 설명)를 저장하는 데이터 구조체
/// </summary>
public class ColumnDefinition
{
    public string Name;
    public string Type;
    public string Desc;
    public string[] values;
}

/// <summary>
/// CSV 파일의 헤더 정보를 저장하는 컨테이너
/// </summary>
public class CsvClassData
{
    public string TableName { get; set; }
    public int DataCount { get; set; }
    public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
}
