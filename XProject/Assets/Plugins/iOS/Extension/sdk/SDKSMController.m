/***************************************
*** SDKSM.h 小7sdk
*** Author: kemingyu
*** Mail: 476734804@qq.com
*** Time: 2018-03-01
***************************************/

#import <SMSDK/SMSDK.h>
#import "SDKSMController.h"
#import "SDKDefaultController.h"

@implementation SDKSMController

-(id) init
{
    self = [super init];
	[self initSDK];
    return self;
}

-(void) Login
{
	[SMSDK smLogin];
}



-(void) OrderAndPay:(NSString *)jsData
{
	if (jsData)
	{
		NSData *data = [jsData dataUsingEncoding:NSUTF8StringEncoding];
		NSError *error = nil;
		NSDictionary *jsonDict = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingAllowFragments error:&error];
		if (error)
		{
			NSLog(@"json解析失败：%@",error);
			return;
		}
		
        NSDictionary *p = @{
						@"game_sign"           :   [jsonDict objectForKey:@"gameSign"],
                        @"game_guid"           :   [jsonDict objectForKey:@"gameGuid"],
                        @"game_orderid"        :   [jsonDict objectForKey:@"extension"],
                        @"game_level"          :   [jsonDict objectForKey:@"roleLevel"],
                        @"game_area"           :   [jsonDict objectForKey:@"serverId"],
                        @"game_price"          :   [jsonDict objectForKey:@"price"],
                        @"game_role_id"        :   [jsonDict objectForKey:@"roleId"],
                        @"game_role_name"      :   [jsonDict objectForKey:@"roleName"],
                        @"subject"             :   [jsonDict objectForKey:@"productDesc"],
                        @"notify_id"           :   @"-1",
                        @"extends_info_data"   :   [jsonDict objectForKey:@"extension"];
                        };
		NSDictionary *params = [self createGameSign:p];
		
		SMPayInfo *payInfo = [[SMPayInfo alloc] init];
		payInfo.game_guid = params[@"game_guid"];
		payInfo.game_orderid = params[@"game_orderid"];
		payInfo.game_level = params[@"game_level"];
		payInfo.game_area = params[@"game_area"];
		payInfo.game_price = params[@"game_price"];
		payInfo.game_role_id = params[@"game_role_id"];
		payInfo.game_role_name = params[@"game_role_name"];
		payInfo.subject = params[@"subject"];
		payInfo.notify_id = params[@"notify_id"];
		payInfo.game_sign = params[@"game_sign"];
		payInfo.extends_info_data = params[@"extends_info_data"];
		[SMSDK smPayWithPayInfo:payInfo];
	}
	else
	{
		[self showAlertView:@"无法解析的数据"];
	}
}

-(void) Logout 
{
    [SMSDK smLogout];
}

-(void) GetSDKInfo
{
	NSLog(@"Get SDK info"); 
	
}

-(void) MemoryWarning
{
	[self SendCallback:"OnMemoryWarning" withParams:nil];
}

- (NSDictionary *)createGameSign:(NSDictionary *)dic {
    NSMutableDictionary *dict = [NSMutableDictionary dictionaryWithDictionary:dic];
    NSString *sign = @"";
    NSArray *keys = [dict allKeys];
    NSArray *sortKeys = [keys sortedArrayUsingSelector:@selector(caseInsensitiveCompare:)];
    for (NSInteger i = 0; i < [sortKeys count]; ++i) {
        NSString *v = [dict objectForKey:[sortKeys objectAtIndex:i]];
        if (i == [sortKeys count] - 1) {
            sign = [sign stringByAppendingString:[NSString stringWithFormat:@"%@=%@", [sortKeys objectAtIndex:i], v]];
        } else {
            sign = [sign stringByAppendingString:[NSString stringWithFormat:@"%@=%@&", [sortKeys objectAtIndex:i], v]];
        }
    }
    sign = [sign stringByAppendingString:PUBLIC_KEY];
    [dict setObject:[self md5HexDigest:sign] forKey:@"game_sign"];
    return dict;
}

- (NSString *)md5HexDigest:(NSString*)input {
    const char* str = [input UTF8String];
    unsigned char result[CC_MD5_DIGEST_LENGTH];
    CC_MD5(str, (CC_LONG)strlen(str), result);
    NSMutableString *ret = [NSMutableString stringWithCapacity:CC_MD5_DIGEST_LENGTH*2];//
    
    for(int i = 0; i<CC_MD5_DIGEST_LENGTH; i++) {
        [ret appendFormat:@"%02x", (unsigned int)(result[i])];
    }
    return ret;
}

-(void)initSDK
{
	[[NSNotificationCenter defaultCenter] removeObserver:self name:SMSDKInitDidFinishNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(SMSDKInitCallback:) name:SMSDKInitDidFinishNotification object:nil];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:SMSDKLoginNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(SMSDKLoginCallback:) name:SMSDKLoginNotification object:nil];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:SMSDKLogoutNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(SMSDKLogoutCallback:) name:SMSDKLogoutNotification object:nil];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self name:SMSDKPayResultNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(SMSDKPayResultCallback:) name:SMSDKPayResultNotification object:nil];
    
    //使用appKey初始化SDK
    [SMSDK smInitWithAppKey:SMSDKAppKey];
}

#pragma mark - SMSDK Notification
- (void)SMSDKInitCallback:(NSNotification *)notify {
    if (notify.object == kSMSDKSuccessResult) {
        //初始化成功, 调用登录方法
        //[SMSDK smLogin];

    } else if (notify.object == kSMSDKFailedResult) {
        
        //初始化失败, 可以重新初始化
        [SMSDK smInitWithAppKey:SMSDKAppKey];
    }
}

- (void)SMSDKLoginCallback:(NSNotification *)notify {
    //登录成功,根据token使用服务端api获取用户唯一标识 guid  (guid获取方法请查看服务端文档)
    if (notify.object == kSMSDKSuccessResult) {
        NSString *token = notify.userInfo[kSMSDKLoginTokenKey];
        NSString *sign = [self md5HexDigest:[NSString stringWithFormat:@"%@%@", SMSDKAppKey, token]];
        
        
    } else {
        
    }
}

- (void)SMSDKLogoutCallback:(NSNotification *)notify {
    NSString *guid = notify.userInfo[kSMSDKLogoutToGuidKey];
    //[SMSDK smLogin];
}

- (void)SMSDKPayResultCallback:(NSNotification *)notify {
    //支付结果
    if (notify.object == kSMSDKSuccessResult) {
        //支付成功, 刷新用户数据
        
    } else if (notify.object == kSMSDKUserCancelResult) {
        //支付取消, 刷新用户数据,保障不漏单
        
    } else if (notify.object == kSMSDKFailedResult) {
        //支付错误, 刷新用户数据,保障不漏单
        NSString *errMsg = notify.userInfo[kSMSDKErrorShowKey];
        errMsg = errMsg && [errMsg isEqualToString:@""] ? errMsg : @"支付失败";
		
    }
}


@end
