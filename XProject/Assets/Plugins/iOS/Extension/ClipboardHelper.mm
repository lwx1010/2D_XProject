#import "ClipboardHelper.h"
@implementation ClipboardHelper

//将文本复制到IOS剪贴板
- (void)objc_copyTextToClipboard : (NSString*)text
{
     UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
     pasteboard.string = text;
}
@end

extern "C" {
     static ClipboardHelper *iosClipboard;

     void _copyToClipboard(const char *textList)
    {
        NSString *text = [NSString stringWithUTF8String: textList] ;

        if(iosClipboard == NULL)
        {
            iosClipboard = [[ClipboardHelper alloc] init];
        }

        [iosClipboard objc_copyTextToClipboard: text];
    }

}