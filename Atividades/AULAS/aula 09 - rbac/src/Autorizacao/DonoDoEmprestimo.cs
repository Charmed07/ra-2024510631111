using System.Security.Claims;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaApi.Autorizacao;

public class DonoDoEmprestimoRequirement : IAuthorizationRequirement { }

public class DonoDoEmprestimoHandler : AuthorizationHandler<DonoDoEmprestimoRequirement, Emprestimo>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DonoDoEmprestimoRequirement requirement,
        Emprestimo emprestimo)
    {
        // 1. Atributo do sujeito: papel -> Bibliotecário renova qualquer empréstimo
        if (context.User.IsInRole("Bibliotecario"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 2. Atributo do sujeito (id no token) vs atributo do recurso (dono)
        var usuarioId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == emprestimo.UsuarioId.ToString())
        {
            context.Succeed(requirement);
        }

        // Sem Succeed = negado por padrão
        return Task.CompletedTask;
    }
}