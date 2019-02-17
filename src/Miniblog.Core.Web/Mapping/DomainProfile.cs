using AutoMapper;
using Miniblog.Core.Contract.Models;
using Miniblog.Core.Data.Models;
using Miniblog.Core.Service.Models;

namespace Miniblog.Core.Web.Mapping
{
    public class DomainProfile : Profile
{
	public DomainProfile()
	{
		CreateMap<Post, PostDto>().ReverseMap();
		CreateMap<Comment, CommentDto>().ReverseMap();
		CreateMap<Category, CategoryDto>().ReverseMap();

		CreateMap<IPost, PostDto>();
		CreateMap<IComment, CommentDto>();
		CreateMap<ICategory, CategoryDto>();

		CreateMap<IPost, Post>();
		CreateMap<IComment, Comment>();
		CreateMap<ICategory, Category>();
	}
}
}