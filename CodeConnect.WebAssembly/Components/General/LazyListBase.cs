using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace CodeConnect.WebAssembly.Components.General;

public class LazyListBase<TItem> : ComponentBase
{
    [Inject] public required IJSRuntime Js { get; set; }
    
    [Parameter] public List<TItem>? Items { get; set; }
    [Parameter] public required RenderFragment<TItem> ListTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public required EventCallback<(int, int)> OnBottomReached { get; set; }
    [Parameter] public int StartIndex { get; set; } = 0;
    [Parameter] public int Take { get; set; } = 50;
    [Parameter] public string? ParentClasses { get; set; }
    
    private bool _noMoreItems = false;
    private int _startIndexUpdated = 0;
    private DotNetObjectReference<LazyListBase<TItem>>? _dotNetRef;
    
    protected readonly string SentinelId = $"sentinel-{Guid.NewGuid()}";
    protected bool Loading = false;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await Js.InvokeVoidAsync("observeSentinel", SentinelId, _dotNetRef);
            _startIndexUpdated = StartIndex;
        }
    }
    [JSInvokable]
    public async Task OnSentinelVisible()
    {
        if (OnBottomReached.HasDelegate && !_noMoreItems)
        {
            SetLoading(true);
            int itemCount = Items?.Count ?? 0;
            await OnBottomReached.InvokeAsync((_startIndexUpdated, Take));
            SetLoading(false);
            _startIndexUpdated += Take;
            if ((Items?.Count ?? 0) == itemCount)
            {
                _noMoreItems = true;
            }
        }
    }
    private void SetLoading(bool isLoading)
    {
        Loading = isLoading;
        StateHasChanged();
    }
    public async ValueTask DisposeAsync()
    {
        await Js.InvokeVoidAsync("unobserveSentinel", SentinelId);
        _dotNetRef?.Dispose();
    }
}