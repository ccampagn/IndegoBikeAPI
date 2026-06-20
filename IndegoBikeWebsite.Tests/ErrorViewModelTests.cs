using IndegoBikeWebsite.Models;

namespace IndegoBikeWebsite.Tests;

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
    {
        var vm = new ErrorViewModel { RequestId = null };
        Assert.False(vm.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_WhenRequestIdIsNotEmpty_ReturnsTrue()
    {
        var vm = new ErrorViewModel { RequestId = "abc-123" };
        Assert.True(vm.ShowRequestId);
    }
}
