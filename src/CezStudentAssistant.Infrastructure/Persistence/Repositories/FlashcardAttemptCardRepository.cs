using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Interfaces.Repositories;
using CezStudentAssistant.Infrastructure.Persistence.Data;

namespace CezStudentAssistant.Infrastructure.Persistence.Repositories;

public class FlashcardAttemptCardRepository(AppDbContext context)
    : GenericRepository<FlashcardAttemptCard>(context), IFlashcardAttemptCardRepository;
