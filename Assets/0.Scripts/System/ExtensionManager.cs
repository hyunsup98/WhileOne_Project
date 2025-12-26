using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 확장 메서드를 추가하는 클래스
/// </summary>
public static class ExtensionManager
{
    /// <summary>
    /// 이미지의 알파값 수정
    /// </summary>
    /// <param name="image"> 알파를 수정할 이미지 </param>
    /// <param name="alpha"> 지정할 알파값 수치</param>
    public static void SetAlpha(this Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
