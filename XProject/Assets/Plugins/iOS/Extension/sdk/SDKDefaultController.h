/***************************************
*** SDKDefault.h
*** Author: luweixing
*** Mail: kinglucn@hotmail.com
*** Time: 2018-01-02 15:06:04
***************************************/
#import <Foundation/Foundation.h>

@interface SDKDefaultController : NSObject

+ (instancetype)shareInstance;

- (void)SendCallback:(const char*)method withParams:(NSDictionary *)params;
- (void)showAlertView:(NSString *)message;
- (void)GetSDKInfo;
- (void)Setup;
- (void)Login;
- (void)UserCenter;
- (void)OrderAndPay:(NSString *)jsData;
- (void)Logout;
- (void)MemoryWarning;

@end
