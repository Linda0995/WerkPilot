using WerkPilot.Domain.Notifications;
namespace WerkPilot.UnitTests;
public sealed class NotificationReadStateTests
{
 [Fact] public void Constructor_StoresUserAndKey(){var user=Guid.NewGuid();var state=new NotificationReadState(user,"project-task:1");Assert.Equal(user,state.UserId);Assert.Equal("project-task:1",state.NotificationKey);}
 [Fact] public void EmptyKey_IsRejected()=>Assert.Throws<ArgumentException>(()=>new NotificationReadState(Guid.NewGuid(),""));
}
