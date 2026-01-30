namespace ConstantLearning.Services;

public enum BotMessageKey
{
    // Registration
    ChatAlreadyRegistered,
    ChatRegisteredSuccess,
    RegistrationError,
    
    // Stop Learning
    ChatNotRegistered,
    LearningStopped,
    StopLearningError,
    
    // Progress
    ProgressRestarted,
    RestartProgressError,
    
    // Time Configuration
    InvalidTimeCommandFormat,
    InvalidTimeFormat,
    RepetitionTimeSet,
    NewWordsTimeSet,
    UpdateTimeError,
    
    // Help
    Help,
    
    // Message Formatter
    NoRepetitionWords,
    RepetitionHeader,
    AnswersHeader,
    NoNewWords,
    NewWordsHeader,
    AllWordsLearned
}
