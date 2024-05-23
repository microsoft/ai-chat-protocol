import React from 'react';
import ReactMarkdown from 'react-markdown';
import gfm from 'remark-gfm'; // for GitHub flavored markdown

const markdown = `
# Title

Some text with **bold** and *italic*.

\`\`\`js
const x1 = 10;
console.log(x1);
\`\`\`
`;

function Readme({ style }: { style: React.CSSProperties }) {
    return (
        <div style={{ overflowY: 'auto', ...style }}>
            <ReactMarkdown remarkPlugins={[gfm]} children={markdown} />
        </div>
    );
}

export default Readme;