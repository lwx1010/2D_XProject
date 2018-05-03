@interface ClipboardHelper : NSObject

extern "C"
{
     /*  compare the namelist with system processes  */
     void _copyToClipboard(const char *textList);
}

@end