// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import ReactMarkdown from 'react-markdown';
import gfm from 'remark-gfm'; // for GitHub flavored markdown
import readmeContent from '../README.md';

function Readme({ style }: { style: React.CSSProperties }) {
    const [markdown, setMarkdown] = useState('');
    useEffect(() => {
        fetch(readmeContent)
            .then(response => response.text())
            .then(text => setMarkdown(text));
    }, []);
    return (
        <div style={{ overflowY: 'auto', ...style }}>
            <ReactMarkdown remarkPlugins={[gfm]}>{markdown}</ReactMarkdown>
        </div>
    );
}

export default Readme;