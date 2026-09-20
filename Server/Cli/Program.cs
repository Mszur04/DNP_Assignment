using Cli;
using FileRepositories;
using RepositoryContracts;

// Program.cs is only responsible for instantiating the repository implementations
// and passing them into the CliApp. This is the single place where the concrete
// repository types are known about, following the Dependency Inversion Principle -
// every other class depends only on the repository interfaces.
// As of Part 3, these are the file-backed repositories, so data now persists
// between runs of the application (previously: UserInMemoryRepository, etc.).
IUserRepository userRepository = new UserFileRepository();
IPostRepository postRepository = new PostFileRepository();
ICommentRepository commentRepository = new CommentFileRepository();

CliApp app = new(userRepository, postRepository, commentRepository);
await app.StartAsync();
