using LoyaltyService.Application.Commands.LoyaltyProgram.CreateLoyaltyProgramCommand;
using LoyaltyService.Application.Commands.LoyaltyProgram.UpdateLoyaltyProgramCommand;
using LoyaltyService.Application.Commands.LoyaltyProgram.DeleteLoyaltyProgramCommand;
using LoyaltyService.Application.Commands.LoyaltyMember.CreateLoyaltyMemberCommand;
using LoyaltyService.Application.Commands.LoyaltyMember.UpdateLoyaltyMemberCommand;
using LoyaltyService.Application.Commands.PointsTransaction.AddPointsCommand;
using LoyaltyService.Application.Commands.PointsTransaction.RedeemPointsCommand;
using LoyaltyService.Application.Commands.Reward.CreateRewardCommand;
using LoyaltyService.Application.Commands.Reward.UpdateRewardCommand;
using LoyaltyService.Application.Commands.Reward.DeleteRewardCommand;
using LoyaltyService.Application.Queries.LoyaltyProgram.GetLoyaltyProgramQuery;
using LoyaltyService.Application.Queries.LoyaltyProgram.GetAllLoyaltyProgramsQuery;
using LoyaltyService.Application.Queries.LoyaltyMember.GetLoyaltyMemberQuery;
using LoyaltyService.Application.Queries.LoyaltyMember.GetAllLoyaltyMembersQuery;
using LoyaltyService.Application.Queries.PointsTransaction.GetPointsTransactionQuery;
using LoyaltyService.Application.Queries.PointsTransaction.GetAllPointsTransactionsQuery;
using LoyaltyService.Application.Queries.Reward.GetRewardQuery;
using LoyaltyService.Application.Queries.Reward.GetAllRewardsQuery;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Commands
            cfg.RegisterServicesFromAssemblyContaining<CreateLoyaltyProgramCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateLoyaltyProgramCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteLoyaltyProgramCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreateLoyaltyMemberCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateLoyaltyMemberCommand>();
            cfg.RegisterServicesFromAssemblyContaining<AddPointsCommand>();
            cfg.RegisterServicesFromAssemblyContaining<RedeemPointsCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreateRewardCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateRewardCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteRewardCommand>();
            
            // Queries
            cfg.RegisterServicesFromAssemblyContaining<GetLoyaltyProgramQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllLoyaltyProgramsQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetLoyaltyMemberQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllLoyaltyMembersQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetPointsTransactionQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllPointsTransactionsQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetRewardQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllRewardsQuery>();
        });

        return services;
    }
}
