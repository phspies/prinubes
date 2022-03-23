for p in $(find . -name *.csproj); do 
	dotnet build $p || exit 1;
done