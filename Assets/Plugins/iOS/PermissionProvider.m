#import "PermissionProviderHelper.h"

extern void _verifyVideoPermission(const char* gameObject, const char* callback)
{
    NSString *NSGameObject = [[NSString alloc] initWithUTF8String:gameObject];
    NSString *NSCallback = [[NSString alloc] initWithUTF8String:callback];
    
	PermissionProviderHelper* permissionProviderHelper = [[PermissionProviderHelper alloc] init];
	[permissionProviderHelper verifyVideoPermission:NSGameObject withCallback:NSCallback];
}

extern void _verifyAudioPermission(const char* gameObject, const char* callback)
{
    NSString *NSGameObject = [[NSString alloc] initWithUTF8String:gameObject];
    NSString *NSCallback = [[NSString alloc] initWithUTF8String:callback];
    
	PermissionProviderHelper* permissionProviderHelper = [[PermissionProviderHelper alloc] init];
	[permissionProviderHelper verifyAudioPermission:NSGameObject withCallback:NSCallback];
}
