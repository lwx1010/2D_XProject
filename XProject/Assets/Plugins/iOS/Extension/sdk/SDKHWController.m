/***************************************
*** SDKHW.m 火舞sdk
*** Author: luweixing
*** Mail: kinglucn@hotmail.com
*** Time: 2018-01-02 16:06:41
***************************************/

#import <HWSDK/HWSDK.h>
#import "SDKHWController.h"
#import "SDKDefaultController.h"

@implementation SDKHWController

-(id) init
{
    self = [super init];
    return self;
}

-(void) Setup
{
	[HWSDK setLogoutCompletionHandler:^{
        NSLog(@"注销回调");
		[self SendCallback:"OnLogout" withParams:nil];
    }];
}

-(void) Login
{
	[HWSDK loginCompletionHandler:^(NSString *userName, NSString *uid, NSString *mobile, NSString *sessionId) {
        NSLog(@"登录成功\n用户名:%@\n用户id:%@\n手机:%@\nsessionId:%@\n",userName,uid,mobile,sessionId);

		NSDictionary *dict = @{@"username":userName,
                            @"userID":uid,  
                            @"mobile":mobile,  
                            @"token":sessionId,
							@"isSuc":@"true"};
		[self SendCallback:"OnLoginSuc" withParams:dict];
    }];
}

-(void) UserCenter
{
	[HWSDK userCenter];
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
		
		NSString *_serverId = [jsonDict objectForKey:@"serverId"];
		NSString *_price = [jsonDict objectForKey:@"price"];
		NSString *_roleId = [jsonDict objectForKey:@"roleId"];
		NSString *_roleName = [jsonDict objectForKey:@"roleName"];
		NSString *_subject = [jsonDict objectForKey:@"productName"];
		NSString *_remark = [NSString stringWithFormat:@"充值%zizz",_subject];
		NSString *_productId = [jsonDict objectForKey:@"productId"];
		NSString *_customInfo = [jsonDict objectForKey:@"productDesc"];
		NSString *_cpOrderId = [jsonDict objectForKey:@"extension"];

        [HWSDK payWithServerId:_serverId price:_price roleId:_roleId roleName:_roleName subject:_subject remark:_remark cpOrderId:_cpOrderId customInfo:_customInfo productId:_productId];
	}
	else
	{
		[self showAlertView:@"无法解析的数据"];
	}
}

-(void) Logout 
{
    [HWSDK logout];
}

-(void) GetSDKInfo
{
	NSLog(@"Get SDK info"); 
	NSString *file = [[NSBundle mainBundle] pathForResource:@"HWSDKRes.bundle/HWParams.plist" ofType:nil];
    //从文件中获取数据,这个从文件中获取了对象并且存储在一个数组中
    NSMutableDictionary *data = [[NSMutableDictionary alloc] initWithContentsOfFile:file];
	NSDictionary *dict = @{@"appID":[data valueForKey:@"gameId"],
                            @"channelID":[data valueForKey:@"cpId"],
                            @"pID":[data valueForKey:@"channelId"]};
	[self SendCallback:"OnGetSDKInfo" withParams:dict];
}

-(void) MemoryWarning
{
	[self SendCallback:"OnMemoryWarning" withParams:nil];
}

@end
