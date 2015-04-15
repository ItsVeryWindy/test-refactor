using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Refactoring;
using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.AnalysisCore.Fixes;
using MonoDevelop.CodeIssues;
using MonoDevelop.Core;
using MonoDevelop.CSharp.Refactoring.CodeActions;
using MonoDevelop.Refactoring;
using MonoDevelop.Refactoring.Rename;
using MonoDevelop.Ide;

namespace TestRefactor
{
	public class TestRefactorIssue : CodeIssueProvider
	{
		public TestRefactorIssue()
		{
			this.Title = "Test Refactor";
			this.Description = "Have yet to decide";
			this.Category = "Code Notifications";
			this.SetMimeType ("text/x-csharp");
			this.IsEnabledByDefault = true;
			this.SetSeverity (Severity.Warning); 
			this.SetIsEnabled (true);
		}
			
		public override bool HasSubIssues {
			get {
				return false;
			}
		}

		public override IEnumerable<CodeIssue> GetIssues (object refactoringContext, System.Threading.CancellationToken cancellationToken)
		{
			var context = refactoringContext as MDRefactoringContext;

			if (context == null || context.IsInvalid || context.RootNode == null || context.ParsedDocument.HasErrors)
				return new CodeIssue[0];
			
			var visitor = new TestRefactorVisitor (this, context);

			context.RootNode.AcceptVisitor(visitor);

			return visitor.Issues;
		}
	}

	class TestRefactorVisitor : DepthFirstAstVisitor
	{
		public readonly List<CodeIssue> Issues = new List<CodeIssue> ();

		private readonly MDRefactoringContext _context;

		private readonly TestRefactorIssue _issue;

		public TestRefactorVisitor (TestRefactorIssue issue, MDRefactoringContext context)
		{
			_issue = issue;
			_context = context;
		}
		 
		public override void VisitNamespaceDeclaration (NamespaceDeclaration namespaceDeclaration)
		{
			Issues.Add (new CodeIssue (IssueMarker.WavedLine,
				"test issue",
				new DomRegion (namespaceDeclaration.NamespaceName.StartLocation, namespaceDeclaration.NamespaceName.EndLocation),
				_issue.IdString,
				GetActions(namespaceDeclaration.NamespaceName as IEntity, _context)
			));

			base.VisitNamespaceDeclaration (namespaceDeclaration);
		}

		private IEnumerable<MonoDevelop.CodeActions.CodeAction> GetActions (IEntity entity, MDRefactoringContext context)
		{
			if (context.IsInvalid)
				yield break;

			yield return new MonoDevelop.CodeActions.DefaultCodeAction ("do something cool", (c, s) => {
				var rename = new RenameRefactoring();

				var options = new RefactoringOptions();

				options.SelectedItem = entity;

				var properties = new RenameRefactoring.RenameProperties();

				properties.NewName = "lolducks";

				var changes = rename.PerformChanges(options, properties);

				var monitor = IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor ("ohnoes", null);

				RefactoringService.AcceptChanges (monitor, changes);
			});
		}
	}
}

