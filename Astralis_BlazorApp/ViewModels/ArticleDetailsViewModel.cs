using Astralis.Shared.DTOs;
using Astralis_BlazorApp.Extensions;
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
        private readonly IReportService _reportService;
        private readonly IReportMotiveService _reportMotiveService;
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
        [ObservableProperty] private CommentDto? commentToDelete;

        [ObservableProperty] private bool isReportModalOpen;
        [ObservableProperty] private CommentDto? commentToReport;
        [ObservableProperty] private List<ReportMotiveDto> reportMotives = new();
        [ObservableProperty] private int selectedReportMotiveId;
        [ObservableProperty] private string reportDescription = "";
        [ObservableProperty] private bool isReportSubmitting;
        [ObservableProperty] private string? reportErrorMessage;
        [ObservableProperty] private bool showReportSuccessMessage;

        public ArticleDetailsViewModel(
            IArticleService articleService,
            ICommentService commentService,
            IReportService reportService,
            IReportMotiveService reportMotiveService,
            AuthenticationStateProvider authStateProvider,
            NavigationManager navigation)
        {
            _articleService = articleService;
            _commentService = commentService;
            _authStateProvider = authStateProvider;
            _reportService = reportService;
            _reportMotiveService = reportMotiveService;
            _navigation = navigation;
        }

        public async Task InitializeAsync(int articleId)
        {
            IsPageLoading = true;
            try
            {
                Article = await _articleService.GetByIdAsync(articleId);

                if (Article != null && Article.IsPremium)
                {
                    var authState = await _authStateProvider.GetAuthenticationStateAsync();
                    var user = authState.User;
                    bool isAuth = user.Identity?.IsAuthenticated ?? false;

                    if (!isAuth)
                    {
                        _navigation.NavigateToLogin(_navigation.Uri);
                        return;
                    }

                    bool isStaff = user.IsInRole("Admin") || user.IsInRole("Rédacteur Commercial");
                    bool isPremiumUser = user.FindFirst("IsPremium")?.Value == "true";

                    if (!isStaff && !isPremiumUser)
                    {
                        _navigation.NavigateTo("/premium");
                        return;
                    }
                }

                IsPageLoading = false;

                var authState2 = await _authStateProvider.GetAuthenticationStateAsync();
                var user2 = authState2.User;
                IsAuthenticated = user2.Identity?.IsAuthenticated ?? false;
                if (IsAuthenticated && int.TryParse(user2.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int uid))
                {
                    CurrentUserId = uid;
                }

                if (ReportMotives.Count == 0)
                {
                    try
                    {
                        ReportMotives = await _reportMotiveService.GetAllAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur chargement motifs: {ex.Message}");
                    }
                }

                AreCommentsLoading = true;
                await LoadCommentsAsync(articleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
                IsPageLoading = false;
            }
            finally
            {
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
                _navigation.NavigateToLogin(_navigation.Uri);
                return; 
            }

            IsCommentBoxOpen = !IsCommentBoxOpen;
            if (!IsCommentBoxOpen)
                NewCommentText = "";
        }

        [RelayCommand]
        public async Task PostCommentAsync()
        {
            if (IsSubmitting)
                return;

            CommentErrorMessage = null;
            var dto = new CommentCreateDto 
            { 
                ArticleId = Article?.Id ?? 0, Text = NewCommentText, RepliesToId = null
            };

            var ctx = new ValidationContext(dto); var res = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, ctx, res, true))
            {
                CommentErrorMessage = res.FirstOrDefault()?.ErrorMessage; 
                return; 
            }

            if (await SubmitCommentInternal(dto)) { 
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

            var ctx = new ValidationContext(dto);
            var res = new List<ValidationResult>();

            if (!Validator.TryValidateObject(dto, ctx, res, true))
            {
                ReplyErrorMessage = res.FirstOrDefault()?.ErrorMessage;
                return;
            }

            if (await SubmitCommentInternal(dto))
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
            catch (Exception ex) { Console.WriteLine($"Erreur post: {ex.Message}"); }
            finally { IsSubmitting = false; }
            return false;
        }

        public void SetReplyingTo(int commentId)
        {
            if (!IsAuthenticated)
            {
                _navigation.NavigateToLogin(_navigation.Uri); 
                return;
            }

            if (ReplyingToCommentId == commentId) {
                ReplyingToCommentId = null; ReplyCommentText = "";
            }
            else 
            {
                ReplyingToCommentId = commentId; ReplyCommentText = "";
            }
        }

        public async Task DeleteArticleAsync()
        {
            if (Article != null) { await _articleService.DeleteAsync(Article.Id); _navigation.NavigateTo("/articles"); }
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
            if (EditingCommentId == null || string.IsNullOrWhiteSpace(EditCommentText))
                return;
            if (EditCommentText.Length > 300)
                return;

            IsSubmitting = true;
            try
            {
                var updateDto = new CommentUpdateDto { Text = EditCommentText };
                await _commentService.UpdateAsync(EditingCommentId.Value, updateDto);

                await LoadCommentsAsync(Article!.Id);

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

        private void RefreshComments()
        {
            Comments = new ObservableCollection<CommentDto>(Comments);
            OnPropertyChanged(nameof(Comments));
            OnPropertyChanged(nameof(TotalCommentCount));
        }

        private CommentDto? FindCommentById(IEnumerable<CommentDto> list, int id)
        {
            foreach (var c in list)
            {
                if (c.Id == id)
                    return c;

                if (c.Replies.Any())
                {
                    var found = FindCommentById(c.Replies, id);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        public void RequestDeleteComment(CommentDto comment) => CommentToDelete = comment;
        public void CancelDeleteComment() => CommentToDelete = null;

        [RelayCommand]
        public async Task ConfirmDeleteCommentAsync()
        {
            if (CommentToDelete == null)
                return;

            var commentIdToDelete = CommentToDelete.Id;
            IsSubmitting = true;

            try
            {
                await _commentService.DeleteAsync(commentIdToDelete);

                await LoadCommentsAsync(Article!.Id);
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
                    return true;
            }

            return false;
        }

        public void OpenReportModal(CommentDto comment)
        {
            if (!IsAuthenticated)
            {
                _navigation.NavigateToLogin(_navigation.Uri);
                return;
            }

            CommentToReport = comment;
            SelectedReportMotiveId = 0;
            ReportDescription = "";
            ReportErrorMessage = null;
            ShowReportSuccessMessage = false;
            IsReportModalOpen = true;
        }

        public void CloseReportModal()
        {
            IsReportModalOpen = false;
            CommentToReport = null;
        }

        [RelayCommand]
        public async Task SubmitReportAsync()
        {
            if (CommentToReport == null) return;
            if (SelectedReportMotiveId == 0)
            {
                ReportErrorMessage = "Veuillez sélectionner un motif.";
                return;
            }

            IsReportSubmitting = true;
            ReportErrorMessage = null;

            try
            {
                var dto = new ReportCreateDto
                {
                    CommentId = CommentToReport.Id,
                    ReportMotiveId = SelectedReportMotiveId,
                    Description = ReportDescription
                };

                await _reportService.AddAsync(dto);

                ShowReportSuccessMessage = true;

                await Task.Delay(1500);
                CloseReportModal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur signalement: {ex.Message}");
                ReportErrorMessage = "Une erreur est survenue lors de l'envoi.";
            }
            finally
            {
                IsReportSubmitting = false;
            }
        }
    }
}