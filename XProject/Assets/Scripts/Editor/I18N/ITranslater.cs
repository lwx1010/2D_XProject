namespace Riverlake.Editor.I18N
{
    /// <summary>
    /// 翻译接口
    /// </summary>
    public interface ITranslater
    {
        TranslateMapper Export(string filePath);
        
        void Translater(TranslateMapper transMap);

    }
}