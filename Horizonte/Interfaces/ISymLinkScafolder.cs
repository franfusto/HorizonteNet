namespace Horizonte;

public interface ISymLinkScafolder
{
    public void CleanScafolder();
    public void BuildScafolder(IEnumerable<SymLinkDef> symlinklist);
    public IEnumerable<SymLinkDef> GetScafolder();

}