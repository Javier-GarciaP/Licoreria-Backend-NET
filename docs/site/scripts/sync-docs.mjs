import { readdir, readFile, writeFile, mkdir, rm, cp } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const sourceDir = path.resolve(__dirname, '../../dev');
const targetDir = path.resolve(__dirname, '../src/content/docs/dev');
const assetsSourceDir = path.resolve(__dirname, '../../assets');
const assetsTargetDir = path.resolve(__dirname, '../src/content/docs/assets');

function extractTitle(content, fallback) {
  const match = content.match(/^\s*#\s+(.+)$/m);
  return match ? match[1].trim() : fallback;
}

function humanize(slug) {
  return slug
    .replace(/[-_]/g, ' ')
    .replace(/\b\w/g, (c) => c.toUpperCase());
}

async function walk(dir) {
  const entries = await readdir(dir, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      files.push(...(await walk(full)));
    } else if (entry.name.endsWith('.md')) {
      files.push(full);
    }
  }
  return files;
}

async function main() {
  if (!existsSync(sourceDir)) {
    console.error(`[sync-docs] No existe el directorio fuente: ${sourceDir}`);
    process.exit(1);
  }

  await rm(targetDir, { recursive: true, force: true });

  const files = await walk(sourceDir);
  let count = 0;

  for (const file of files) {
    let rel = path.relative(sourceDir, file).split(path.sep).join('/');
    rel = rel.replace(/README\.md$/i, 'index.md');

    const outPath = path.join(targetDir, rel);
    await mkdir(path.dirname(outPath), { recursive: true });

    const raw = (await readFile(file, 'utf8')).replace(/^\uFEFF/, '');

    if (raw.trimStart().startsWith('---')) {
      await writeFile(outPath, raw, 'utf8');
    } else {
      const slug = path.basename(rel, '.md');
      const fallback =
        slug === 'index' ? path.basename(path.dirname(rel)) : slug;
      const title = extractTitle(raw, humanize(fallback));
      const frontmatter = `---\ntitle: ${JSON.stringify(title)}\n---\n\n`;
      await writeFile(outPath, frontmatter + raw, 'utf8');
    }
    count += 1;
  }

  console.log(
    `[sync-docs] ${count} documentos sincronizados desde docs/dev a src/content/docs/dev`,
  );

  if (existsSync(assetsSourceDir)) {
    await rm(assetsTargetDir, { recursive: true, force: true });
    await cp(assetsSourceDir, assetsTargetDir, { recursive: true });
    console.log('[sync-docs] Assets copiados desde docs/assets a src/content/docs/assets');
  }
}

main().catch((err) => {
  console.error('[sync-docs] Error:', err);
  process.exit(1);
});
