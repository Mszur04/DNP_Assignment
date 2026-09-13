using Cli;
using InMemoryRepositories;
using RepositoryContracts;

// Program.cs is only responsible for instantiating the repository implementations
// and passing them into the CliApp. This is the single place where the concrete
// repository types are known about, following the Dependency Inversion Principle -
// every other class depends only on the repository interfaces.
IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp app = new(userRepository, postRepository, commentRepository);
await app.StartAsync();
