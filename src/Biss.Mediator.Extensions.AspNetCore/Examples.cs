using Biss.Mediator.Abstractions;
using Biss.Mediator.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Biss.Mediator.Extensions.AspNetCore.Examples;

// Este arquivo contém exemplos de uso do MediatorControllerBase
// Estes são apenas exemplos de referência, não devem ser compilados

#region Exemplo 1: Controller Básico

/*
public class UsersController : MediatorControllerBase
{
    public UsersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<UserId>> CreateUser(CreateUserCommand command)
    {
        return await Send(command);
    }
}
*/

#endregion

#region Exemplo 2: Controller com Múltiplos Endpoints

/*
[ApiController]
[Route("api/[controller]")]
public class ProductsController : MediatorControllerBase
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<ProductId>> Create(CreateProductCommand command)
    {
        // Retorna 201 Created automaticamente se usar status code customizado
        return await Send(command, StatusCodes.Status201Created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var query = new GetProductQuery(id);
        return await Send(query); // Retorna 200 OK em sucesso, ou erro apropriado
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateProductCommand command)
    {
        return await Send(command); // Retorna 200 OK em sucesso
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteProductCommand(id);
        return await Send(command); // Retorna 200 OK em sucesso
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List(
        [FromQuery] GetProductsQuery query)
    {
        return await Send(query);
    }
}
*/

#endregion

#region Exemplo 3: Customização de Mapeamento de Erros

/*
public class CustomMediatorControllerBase : MediatorControllerBase
{
    public CustomMediatorControllerBase(IMediator mediator) : base(mediator)
    {
    }

    protected override ActionResult MapErrorToActionResult(Error error)
    {
        // Personalizar o mapeamento de erros
        return error switch
        {
            NotFoundError notFound => NotFound(new
            {
                error.Code,
                error.Message,
                Timestamp = DateTime.UtcNow
            }),
            ValidationError validation => BadRequest(new
            {
                error.Code,
                error.Message,
                Field = validation.Field,
                Timestamp = DateTime.UtcNow
            }),
            _ => base.MapErrorToActionResult(error)
        };
    }
}
*/

#endregion

#region Exemplo 4: Uso de Notificações

/*
[ApiController]
[Route("api/[controller]")]
public class OrdersController : MediatorControllerBase
{
    public OrdersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<ActionResult<OrderId>> CreateOrder(CreateOrderCommand command)
    {
        var result = await Send(command);
        
        if (result.Result is OkObjectResult okResult && okResult.Value is OrderId orderId)
        {
            // Publicar notificação após criar o pedido
            await Publish(new OrderCreatedNotification(orderId));
        }
        
        return result;
    }
}
*/

#endregion

#region Exemplo 5: Uso com HttpContext Extensions

/*
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obter Mediator do HttpContext
        var mediator = context.GetMediator();
        
        // Usar o mediator para enviar requests
        // Por exemplo, para logging ou auditoria
        await _next(context);
    }
}
*/

#endregion

