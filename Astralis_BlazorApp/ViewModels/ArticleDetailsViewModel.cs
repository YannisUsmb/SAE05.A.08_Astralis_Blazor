using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Astralis_BlazorApp.ViewModels
{
    public partial class ArticleDetailsViewModel : ObservableObject
    {
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _navigation;

        [ObservableProperty] private ArticleDetailDto? article;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalCommentCount))]
        private ObservableCollection<CommentDto> comments = new();

        public int TotalCommentCount => Comments.Sum(c => 1 + CountReplies(c));
        private int CountReplies(CommentDto c) => c.Replies.Sum(r => 1 + CountReplies(r));

        [ObservableProperty] private bool isPageLoading = true;
        [ObservableProperty] private bool areCommentsLoading = true;

        [ObservableProperty] private bool isSubmitting = false;

        [ObservableProperty] private bool isAuthenticated;
        [ObservableProperty] private int currentUserId;

        [ObservableProperty] private string newCommentText = "";
        [ObservableProperty] private bool isCommentBoxOpen;
        [ObservableProperty] private string? commentErrorMessage;

        [ObservableProperty] private int? replyingToCommentId;
        [ObservableProperty] private string replyCommentText = "";
        [ObservableProperty] private string? replyErrorMessage;

        [ObservableProperty] private int? editingCommentId;
        [ObservableProperty] private string editCommentText = "";

        [ObservableProperty]
        private CommentDto? commentToDelete;

        public ArticleDetailsViewModel(
            IArticleService articleService,
            ICommentService commentService,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation)
        {
            _articleService = articleService;
            _commentService = commentService;
            _authStateProvider = authStateProvider;
            _navigation = navigation;
        }

        public async Task InitializeAsync(int articleId)
        {
            IsPageLoading = true;
            try
            {
                Article = await _articleService.GetByIdAsync(articleId);
                IsPageLoading = false;

                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
                if (IsAuthenticated && int.TryParse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int uid))
                {
                    CurrentUserId = uid;
                }

                AreCommentsLoading = true;
                await LoadCommentsAsync(articleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
            finally
            {
                IsPageLoading = false;
                AreCommentsLoading = false;
            }
        }

        private async Task LoadCommentsAsync(int articleId)
        {
            var commentsList = await _commentService.GetByArticleIdAsync(articleId);
            Comments = new ObservableCollection<CommentDto>(commentsList);
        }

        public void ToggleCommentBox()
        {
            if (!IsAuthenticated)
            {
                _navigation.NavigateTo("/login");
                return;
            }
            IsCommentBoxOpen = !IsCommentBoxOpen;
            if (!IsCommentBoxOpen) NewCommentText = "";
        }

        [RelayCommand]
        public async Task PostCommentAsync()
        {
            if (IsSubmitting) return;

            CommentErrorMessage = null;

            var dto = new CommentCreateDto
            {
                ArticleId = Article?.Id ?? 0,
                Text = NewCommentText,
                RepliesToId = null
            };

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                CommentErrorMessage = validationResults.FirstOrDefault()?.ErrorMessage;
                return;
            }

            var success = await SubmitCommentInternal(dto);
            if (success)
            {
                NewCommentText = "";
                IsCommentBoxOpen = false;
            }
        }

        [RelayCommand]
        public async Task PostReplyAsync(int parentId)
        {
            ReplyErrorMessage = null;

            var dto = new CommentCreateDto
            {
                ArticleId = Article?.Id ?? 0,
                Text = ReplyCommentText,
                RepliesToId = parentId
            };

            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                ReplyErrorMessage = validationResults.FirstOrDefault()?.ErrorMessage;
                return;
            }

            var success = await SubmitCommentInternal(dto);
            if (success)
            {
                ReplyCommentText = "";
                ReplyingToCommentId = null;
            }
        }

        private async Task<bool> SubmitCommentInternal(CommentCreateDto dto)
        {
            IsSubmitting = true;
            try
            {
                var created = await _commentService.AddAsync(dto);
                if (created != null)
                {
                    await LoadCommentsAsync(Article!.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur post: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
            return false;
        }

        public void SetReplyingTo(int commentId)
        {
            if (!IsAuthenticated)
            {
                _navigation.NavigateTo("/login");
                return;
            }

            if (ReplyingToCommentId == commentId)
            {
                ReplyingToCommentId = null;
                ReplyCommentText = "";
            }
            else
            {
                ReplyingToCommentId = commentId;
                ReplyCommentText = "";
            }
        }

        public async Task DeleteArticleAsync()
        {
            if (Article != null)
            {
                await _articleService.DeleteAsync(Article.Id);
                _navigation.NavigateTo("/articles");
            }
        }

        public void StartEditing(CommentDto comment)
        {
            EditingCommentId = comment.Id;
            EditCommentText = comment.Text;

            ReplyingToCommentId = null;
        }

        public void CancelEditing()
        {
            EditingCommentId = null;
            EditCommentText = "";
        }

        [RelayCommand]
        public async Task SaveEditAsync()
        {
            if (EditingCommentId == null || string.IsNullOrWhiteSpace(EditCommentText)) return;

            if (EditCommentText.Length > 300) return;

            IsSubmitting = true;
            try
            {
                var updateDto = new CommentUpdateDto { Text = EditCommentText };
                await _commentService.UpdateAsync(EditingCommentId.Value, updateDto);

                var commentToUpdate = FindCommentById(Comments, EditingCommentId.Value);
                if (commentToUpdate != null)
                {
                    commentToUpdate.Text = EditCommentText;
                    commentToUpdate.IsEdited = true;

                    Comments = new ObservableCollection<CommentDto>(Comments);
                }

                CancelEditing();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur edit: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private CommentDto? FindCommentById(IEnumerable<CommentDto> list, int id)
        {
            foreach (var c in list)
            {
                if (c.Id == id) return c;
                if (c.Replies.Any())
                {
                    var found = FindCommentById(c.Replies, id);
                    if (found != null) return found;
                }
            }
            return null;
        }

        public void RequestDeleteComment(CommentDto comment)
        {
            CommentToDelete = comment;
        }

        public void CancelDeleteComment()
        {
            CommentToDelete = null;
        }

        [RelayCommand]
        public async Task ConfirmDeleteCommentAsync()
        {
            if (CommentToDelete == null) return;

            IsSubmitting = true;
            try
            {
                await _commentService.DeleteAsync(CommentToDelete.Id);

                if (RemoveCommentLocally(Comments, CommentToDelete.Id))
                {
                    Comments = new ObservableCollection<CommentDto>(Comments);
                    OnPropertyChanged(nameof(TotalCommentCount));
                }
                else
                {
                    await LoadCommentsAsync(Article!.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur delete: {ex.Message}");
            }
            finally
            {
                CommentToDelete = null;
                IsSubmitting = false;
            }
        }

        private bool RemoveCommentLocally(IList<CommentDto> list, int idToDelete)
        {
            var item = list.FirstOrDefault(c => c.Id == idToDelete);
            if (item != null)
            {
                list.Remove(item);
                return true;
            }

            foreach (var child in list)
            {
                if (RemoveCommentLocally(child.Replies, idToDelete))
                {
                    return true;
                }
            }
            return false;
        }
    }
}